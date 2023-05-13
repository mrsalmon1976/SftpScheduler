using Microsoft.Extensions.Logging;
using SftpSchedulerService.AutoUpdater.Config;
using SftpSchedulerService.AutoUpdater.Services;
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

        public UpdateOrchestrator(ILogger<UpdateOrchestrator> logger,
            AppSettings appSettings,
            UpdateLocationInfo updateLocationInfo,
            IVersionComparisonService versionComparisonService)
        {
            _logger = logger;
            _appSettings = appSettings;
            _updateLocationInfo = updateLocationInfo;
            _versionComparisonService = versionComparisonService;
        }

        public async Task<bool> Run()
        {
            _logger.LogInformation("Checking for new version (Application path: {0})", _updateLocationInfo.ApplicationFolder);
            VersionComparisonResult versionComparisonResult = await _versionComparisonService.CheckIfNewVersionAvailable(_appSettings.LatestVersionUrl, _updateLocationInfo.ApplicationFolder);
            return true;
        }
    }
}
