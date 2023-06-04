using SftpScheduler.Common.Models;

namespace SftpScheduler.Common.Services
{
    public interface IVersionComparisonService
    {
        Task<VersionComparisonResult> CheckIfNewVersionAvailable(string latestVersionUrl, string applicationFolder);
    }

    public class VersionComparisonService : IVersionComparisonService
    {
        private readonly IApplicationVersionService _appVersionService;
        private readonly IGitHubVersionService _gitHubVersionService;

        public VersionComparisonService(IApplicationVersionService appVersionService, IGitHubVersionService gitHubVersionService)
        {
            this._appVersionService = appVersionService;
            this._gitHubVersionService = gitHubVersionService;
        }

        public async Task<VersionComparisonResult> CheckIfNewVersionAvailable(string latestVersionUrl, string applicationFolder)
        {
            string installedVersion = _appVersionService.GetVersion(applicationFolder);
            ApplicationVersionInfo versionInfo = await _gitHubVersionService.GetVersionInfo(latestVersionUrl);
            string latestReleaseVersion = versionInfo.VersionNumber!;

            var vInstalled = Version.Parse(installedVersion);
            var vLatest = Version.Parse(latestReleaseVersion);

            VersionComparisonResult result = new VersionComparisonResult();
            result.IsNewVersionAvailable = (vInstalled < vLatest);
            result.LatestReleaseVersionInfo = versionInfo;
            return result;
        }


    }
}
