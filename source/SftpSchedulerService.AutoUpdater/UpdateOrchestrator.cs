using Microsoft.Extensions.Logging;
using SftpSchedulerService.AutoUpdater.Config;
using SftpSchedulerService.AutoUpdater.Services;
using SftpSchedulerService.Common;
using SftpSchedulerService.Common.Models;
using SftpSchedulerService.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.AutoUpdater
{
    internal class UpdateOrchestrator
    {
        private readonly ILogger<UpdateOrchestrator> _logger;
        private readonly AppSettings _appSettings;
        private readonly UpdateLocationInfo _updateLocationInfo;
        private readonly IVersionComparisonService _versionComparisonService;
        private readonly IUpdateFileService _updateFileService;
        private readonly IUpdateDownloadService _updateDownloadService;
        private readonly IInstallationService _installationService;

        public UpdateOrchestrator(ILogger<UpdateOrchestrator> logger,
            AppSettings appSettings,
            UpdateLocationInfo updateLocationInfo,
            IVersionComparisonService versionComparisonService,
            IUpdateFileService updateFileService,
            IUpdateDownloadService updateDownloadService,
            IInstallationService installationService
            )
        {
            _logger = logger;
            _appSettings = appSettings;
            _updateLocationInfo = updateLocationInfo;
            _versionComparisonService = versionComparisonService;
            _updateFileService = updateFileService;
            _updateDownloadService = updateDownloadService;
            _installationService = installationService;
        }

        public async Task<bool> Run()
        {
            _logger.LogInformation("Checking for new version (Application path: {0})", _updateLocationInfo.ApplicationFolder);
            VersionComparisonResult versionComparisonResult = await _versionComparisonService.CheckIfNewVersionAvailable(_appSettings.LatestVersionUrl, _updateLocationInfo.ApplicationFolder);
            bool updated = false;

            ApplicationVersionInfo latestVersionInfo = versionComparisonResult.LatestReleaseVersionInfo!;

            if (versionComparisonResult.IsNewVersionAvailable)
            {
                _logger.LogInformation("New version available: {0}", latestVersionInfo.VersionNumber);
                updated = true;

                _logger.LogInformation("Creating temporary update folder {0}", _updateLocationInfo.UpdateTempFolder);
                //_updateFileService.EnsureEmptyUpdateTempFolderExists(_updateLocationInfo.UpdateTempFolder);

                _logger.LogInformation("Downloading file from {0}", latestVersionInfo.DownloadUrl);
                string downloadPath = Path.Combine(_updateLocationInfo.UpdateTempFolder, latestVersionInfo.FileName!);
                //await _updateDownloadService.DownloadFile(latestVersionInfo.DownloadUrl!, downloadPath);

                _logger.LogInformation("Extracting release contents to {0}", _updateLocationInfo.UpdateTempFolder);
                //await _updateFileService.ExtractReleasePackage(downloadPath, _updateLocationInfo.UpdateTempFolder);

                if (_installationService.IsServiceInstalled())
                {
                    _logger.LogInformation("Stopping {0} service", UpdateConstants.ServiceName);
                    _installationService.StopService();

                    _logger.LogInformation("Uninstalling {0} service", UpdateConstants.ServiceName);
                    _installationService.UninstallService();
                }

                _logger.LogInformation("Backing up current service files.");
                await _updateFileService.Backup(_updateLocationInfo);

                //_updateEventLogger.Log("Deleting current service files...");
                //await _updateFileService.DeleteCurrentVersionFiles();
                //_updateEventLogger.LogLine("done.");

                //_updateEventLogger.Log("Copying new service files...");
                //await _updateFileService.CopyNewVersionFiles(latestVersionInfo.FileName);
                //_updateEventLogger.LogLine("done.");

                _logger.LogInformation("Installing {0} service", UpdateConstants.ServiceName);
                _installationService.InstallService(_updateLocationInfo.ApplicationFolder);

                _logger.LogInformation("Starting {0} service", UpdateConstants.ServiceName);
                _installationService.StartService();

                //// cleanup!
                //_updateEventLogger.Log($"Cleaning up temp update folder {_updateLocationService.UpdateTempFolder}...");
                //_updateLocationService.DeleteUpdateTempFolder();
                //_updateEventLogger.LogLine("done.");

            }
            else 
            {
                _logger.LogInformation("Latest version already installed ({0})", latestVersionInfo.VersionNumber);
            }

            return updated;
        }
    }
}
