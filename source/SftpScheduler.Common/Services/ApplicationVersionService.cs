using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.Common.Services
{
    public interface IApplicationVersionService
    {
        string GetVersion(string applicationFolder);
    }

    public class ApplicationVersionService : IApplicationVersionService
    {
        public string GetVersion(string applicationFolder)
        {
            string pathToExe = Path.Combine(applicationFolder, UpdateConstants.ApplicationExeFileName);
            if (!File.Exists(pathToExe))
            {
                throw new FileNotFoundException(String.Format("Unable to find web console executable {0}", pathToExe));
            }
            var versionInfo = FileVersionInfo.GetVersionInfo(pathToExe);
            return System.Version.Parse(versionInfo.FileVersion!).ToString(3);
        }
    }
}
