﻿using Microsoft.Extensions.Logging;
using SftpSchedulerServiceUpdater.Config;
using SftpSchedulerServiceUpdater.Services;
using SftpScheduler.Common;
using SftpScheduler.Common.Models;
using SftpScheduler.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerServiceUpdater
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
                _logger.LogInformation("New version available: {versionNumber}", latestVersionInfo.VersionNumber);
                updated = true;

                _logger.LogInformation("Updater location information:");
                _logger.LogInformation("..........Application folder: {0}", _updateLocationInfo.ApplicationFolder);
                _logger.LogInformation("..........Log folder: {0}", _updateLocationInfo.LogFolder);
                _logger.LogInformation("..........Data folder: {0}", _updateLocationInfo.DataFolder);
                _logger.LogInformation("..........Backup folder: {0}", _updateLocationInfo.BackupFolder);
                _logger.LogInformation("..........Updater folder: {0}", _updateLocationInfo.UpdaterFolder);
                _logger.LogInformation("..........UpdateTemp folder: {0}", _updateLocationInfo.UpdateTempFolder);

                _logger.LogInformation("Creating temporary update folder {updateTempFolder}", _updateLocationInfo.UpdateTempFolder);
                _updateFileService.EnsureEmptyUpdateTempFolderExists(_updateLocationInfo.UpdateTempFolder);

                _logger.LogInformation("Downloading file from {downloadUrl}", latestVersionInfo.DownloadUrl);
                string downloadPath = Path.Combine(_updateLocationInfo.UpdateTempFolder, latestVersionInfo.FileName!);
                await _updateDownloadService.DownloadFile(latestVersionInfo.DownloadUrl!, downloadPath);

                _logger.LogInformation("Extracting release contents to {updateTempFolder}", _updateLocationInfo.UpdateTempFolder);
                _updateFileService.ExtractReleasePackage(downloadPath, _updateLocationInfo.UpdateTempFolder);

                bool isServiceInstalled = _installationService.IsServiceInstalled();

                if (isServiceInstalled)
                {
                    _logger.LogInformation("Stopping {serviceName} service", UpdateConstants.ServiceName);
                    _installationService.StopService();

                    // we don't uninstall completely as we don't want to lose service login details
                    //_logger.LogInformation("Uninstalling {serviceName} service", UpdateConstants.ServiceName);
                    //_installationService.UninstallService();

                }

                _logger.LogInformation("Backing up current service files.");
                _updateFileService.Backup(_updateLocationInfo);

                _logger.LogInformation("Deleting current service files.");
                _updateFileService.DeleteCurrentVersionFiles(_updateLocationInfo);

                _logger.LogInformation("Copying new service files...");
                _updateFileService.CopyNewVersionFiles(_updateLocationInfo);

                // if the service was not installed, install it now!
                if (!isServiceInstalled)
                {
                    _logger.LogInformation("Installing {serviceName} service", UpdateConstants.ServiceName);
                    _installationService.InstallService(_updateLocationInfo.ApplicationFolder);
                }

                _logger.LogInformation("Starting {serviceName} service", UpdateConstants.ServiceName);
                _installationService.StartService();

                _logger.LogInformation("Cleaning up temp update folder {updateTempFolder}.", _updateLocationInfo.UpdateTempFolder);
                _updateFileService.DeleteUpdateTempFolder(_updateLocationInfo.UpdateTempFolder);

            }
            else 
            {
                _logger.LogInformation("Latest version already installed ({versionNumber})", latestVersionInfo.VersionNumber);
            }

            return updated;
        }
    }
}
