using SftpSchedulerService.AutoUpdater.Config;
using SftpSchedulerService.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.AutoUpdater.Services
{
    public class UpdateLocationInfo
    {
        private string _applicationFolder = String.Empty;

        public UpdateLocationInfo()
        {
            DirectoryInfo di = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            if (Debugger.IsAttached)
            {
                string debugAppFolder = Path.Combine(di?.Parent?.Parent?.Parent?.FullName, "SftpSchedulerService\\bin\\Debug\\net6.0");
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
                this.AutoUpdaterFolder = Path.Combine(this._applicationFolder, UpdateConstants.AutoUpdaterFolderName);
            }
        }

        public string DataFolder { get; private set; } = String.Empty;

        public string LogFolder { get; private set; } = String.Empty;

        public string UpdateTempFolder { get; private set; } = String.Empty;

        public string AutoUpdaterFolder { get; private set; } = String.Empty;

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
