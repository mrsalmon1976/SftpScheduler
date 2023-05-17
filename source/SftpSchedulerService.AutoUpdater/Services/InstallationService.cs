#pragma warning disable CA1416
using Microsoft.Extensions.Logging;
using SftpSchedulerService.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.AutoUpdater.Services
{
    public interface IInstallationService
    {
        void InstallService(string applicationFolder);

        bool IsServiceInstalled();

        void StartService();

        void StopService();

        void UninstallService();
    }

    public class InstallationService : IInstallationService
    {
        private readonly ILogger<InstallationService> _logger;

        public InstallationService(ILogger<InstallationService> logger)
        {
            _logger = logger;
        }

        private ServiceController? GetInstalledService()
        {
            ServiceController[] services = ServiceController.GetServices();
            return services.FirstOrDefault(x => x.ServiceName == UpdateConstants.ServiceName);
        }

        public void InstallService(string applicationFolder)
        {
            string servicePath = Path.Combine(applicationFolder, UpdateConstants.ApplicationExeFileName);
            servicePath = String.Format("\"{0}\"", servicePath);
            _logger.LogDebug("Service path for installation: {0}", servicePath);
            this.RunProcess(applicationFolder
                , "create", UpdateConstants.ServiceName
                , "binpath=", servicePath
                , "start=", "auto");
        }


        public bool IsServiceInstalled() 
        {
            ServiceController? sc = this.GetInstalledService();
            return (sc != null);
        }

        public void StartService()
        {
            ServiceController? sc = GetInstalledService();
            if (sc == null)
            {
                throw new ApplicationException("Service start called, but service is not installed.");
            }
            else
            {
                this.RunProcess(AppDomain.CurrentDomain.BaseDirectory, "start", UpdateConstants.ServiceName);
            }
        }

        public void StopService()
        {
            ServiceController? sc = GetInstalledService();
            if (sc != null && sc.Status == ServiceControllerStatus.Running)
            {
                this.RunProcess(AppDomain.CurrentDomain.BaseDirectory, "stop", UpdateConstants.ServiceName);
            }
        }

        public void UninstallService()
        {
            this.RunProcess(AppDomain.CurrentDomain.BaseDirectory, "delete", UpdateConstants.ServiceName);
        }

        private void RunProcess(string workingDir, params string[] processArguments)
        {
            string args = String.Join(" ", processArguments);

            _logger.LogDebug("RunProcess called with workingDir [{workingDir}], arguments [{args}]", workingDir, args);

            using (Process cmd = new Process())
            {
                cmd.StartInfo.WorkingDirectory = workingDir;
                cmd.StartInfo.FileName = "sc.exe";
                cmd.StartInfo.Arguments = args;
                cmd.StartInfo.Verb = "runas";
                cmd.StartInfo.UseShellExecute = true;
                cmd.Start();
                cmd.WaitForExit();
                if (cmd.ExitCode != 0)
                {
                    throw new ApplicationException(String.Format("Process exited with error code {0}", cmd.ExitCode));
                }
            }
        }
    }
}
