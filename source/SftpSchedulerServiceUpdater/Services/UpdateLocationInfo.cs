using SftpScheduler.Common;
using System.Diagnostics;
using System.Reflection;

namespace SftpSchedulerServiceUpdater.Services
{
    public class UpdateLocationInfo
    {
        private string _applicationFolder = String.Empty;

        public UpdateLocationInfo()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            DirectoryInfo di = Directory.GetParent(Path.GetDirectoryName(assemblyPath!)!)!;
            if (Debugger.IsAttached)
            {
                string debugAppFolder = Path.Combine(di?.Parent?.Parent?.Parent?.FullName!, "SftpSchedulerService\\bin\\Debug\\net6.0");
                di =  new DirectoryInfo(debugAppFolder);
            }
            this.ApplicationFolder = di.FullName;
        }

        public string BackupFolder { get; private set; } = String.Empty;

        public string ApplicationFolder
        {
            get
            {
                return _applicationFolder;
            }
            set
            {
                this._applicationFolder = value;

                this.BackupFolder = Path.Combine(this._applicationFolder, UpdateConstants.BackupFolderName);
                this.DataFolder = Path.Combine(this._applicationFolder, UpdateConstants.DataFolderName);
                this.LogFolder = Path.Combine(this._applicationFolder, UpdateConstants.LogFolderName);
                this.UpdateTempFolder = Path.Combine(this._applicationFolder, UpdateConstants.UpdateTempFolderName);
                this.UpdaterFolder = Path.Combine(this._applicationFolder, UpdateConstants.UpdaterFolderName);
            }
        }

        public string DataFolder { get; private set; } = String.Empty;

        public string LogFolder { get; private set; } = String.Empty;

        public string UpdateTempFolder { get; private set; } = String.Empty;

        public string UpdaterFolder { get; private set; } = String.Empty;

        //public void DeleteUpdateTempFolder()
        //{
        //    if (Directory.Exists(this.UpdateTempFolder))
        //    {
        //        Directory.Delete(this.UpdateTempFolder, true);
        //    }
        //}

        //public void EnsureEmptyUpdateTempFolderExists()
        //{
        //    this.DeleteUpdateTempFolder();
        //    Directory.CreateDirectory(this.UpdateTempFolder);
        //}
    }

    
}
