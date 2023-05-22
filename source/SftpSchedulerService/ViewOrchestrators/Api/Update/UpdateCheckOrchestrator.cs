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
        private readonly IVersionComparisonService _versionComparisonService;
        private readonly ICacheProvider _cacheProvider;
        private readonly AppSettings _appSettings;

        public UpdateCheckOrchestrator(IVersionComparisonService versionComparisonService, ICacheProvider cacheProvider, AppSettings appSettings)
        {
            _versionComparisonService = versionComparisonService;
            _cacheProvider = cacheProvider;
            _appSettings = appSettings;
        }

        public async Task<IActionResult> Execute()
        {
            VersionCheckViewModel viewModel = _cacheProvider.Get<VersionCheckViewModel>(CacheKeys.VersionUpdateCheck);

            if (viewModel == null)
            {
                VersionComparisonResult result = await _versionComparisonService.CheckIfNewVersionAvailable(_appSettings.LatestVersionUrl, _appSettings.BaseDirectory);
                viewModel = new VersionCheckViewModel();
                viewModel.IsNewVersionAvailable = result.IsNewVersionAvailable;
                viewModel.LatestReleaseVersionNumber = result.LatestReleaseVersionInfo!.VersionNumber;
                _cacheProvider.Set(CacheKeys.VersionUpdateCheck, viewModel, TimeSpan.FromMinutes(_appSettings.UpdateCheckIntervalInMinutes));
            }

            return new OkObjectResult(viewModel);
        }
    }
}
