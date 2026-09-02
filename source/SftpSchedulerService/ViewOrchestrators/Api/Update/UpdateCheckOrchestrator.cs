using Microsoft.AspNetCore.Mvc;
using SftpSchedulerService.Caching;
using SftpScheduler.Common.Models;
using SftpScheduler.Common.Services;
using SftpSchedulerService.Config;
using SftpSchedulerService.Models.Update;

namespace SftpSchedulerService.ViewOrchestrators.Api.Update
{
    public interface IUpdateCheckOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute();
    }

    public class UpdateCheckOrchestrator : IUpdateCheckOrchestrator
    {
        private readonly ILogger<UpdateCheckOrchestrator> _logger;
        private readonly IVersionComparisonService _versionComparisonService;
        private readonly ICacheProvider _cacheProvider;
        private readonly AppSettings _appSettings;

        public UpdateCheckOrchestrator(ILogger<UpdateCheckOrchestrator> logger, IVersionComparisonService versionComparisonService, ICacheProvider cacheProvider, AppSettings appSettings)
        {
            _logger = logger;
            _versionComparisonService = versionComparisonService;
            _cacheProvider = cacheProvider;
            _appSettings = appSettings;
        }

        public async Task<IActionResult> Execute()
        {
            VersionCheckViewModel viewModel = _cacheProvider.Get<VersionCheckViewModel>(CacheKeys.VersionUpdateCheck);

            if (viewModel == null)
            {
                try
                {
                    VersionComparisonResult result = await _versionComparisonService.CheckIfNewVersionAvailable(_appSettings.LatestVersionUrl, _appSettings.BaseDirectory);
                    viewModel = new VersionCheckViewModel();
                    viewModel.IsNewVersionAvailable = result.IsNewVersionAvailable;
                    viewModel.LatestReleaseVersionNumber = result.LatestReleaseVersionInfo?.VersionNumber;
                    _cacheProvider.Set(CacheKeys.VersionUpdateCheck, viewModel, TimeSpan.FromMinutes(_appSettings.UpdateCheckIntervalInMinutes));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to load version information from GitHub.");

                    // we don't want to be checking all the time - so here we still cache the version 
                    // check for later
                    viewModel = new VersionCheckViewModel();
                    viewModel.IsNewVersionAvailable = false;
                    viewModel.LatestReleaseVersionNumber = null;
                    _cacheProvider.Set(CacheKeys.VersionUpdateCheck, viewModel, TimeSpan.FromMinutes(_appSettings.UpdateCheckIntervalInMinutes));

                    return new ObjectResult("Unable to load version information from GitHub.")
                    {
                        StatusCode = 500
                    };
                }
            }
            //viewModel.IsNewVersionAvailable = true;
            return new OkObjectResult(viewModel);
        }
    }
}
