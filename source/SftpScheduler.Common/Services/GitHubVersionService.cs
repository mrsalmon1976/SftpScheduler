﻿using Newtonsoft.Json;
using SftpScheduler.Common.Models;
using SftpScheduler.Common.Web;

namespace SftpScheduler.Common.Services
{
    public interface IGitHubVersionService
    {
        Task<ApplicationVersionInfo> GetVersionInfo(string latestVersionUrl);
    }

    public class GitHubVersionService : IGitHubVersionService
    {

        private readonly IHttpClientFactory _httpClientFactory;

        public GitHubVersionService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ApplicationVersionInfo> GetVersionInfo(string latestVersionUrl)
        {
            HttpClient client = _httpClientFactory.GetHttpClient();
            var result = await client.GetAsync(latestVersionUrl);
            result.EnsureSuccessStatusCode();
            var body = await result.Content.ReadAsStringAsync();
            GitHubReleaseResponse releaseData = JsonConvert.DeserializeObject<GitHubReleaseResponse>(body)!;
            ApplicationVersionInfo versionInfo = new ApplicationVersionInfo();
            versionInfo.VersionNumber = releaseData.TagName.TrimStart('v');

            GitHubAsset? asset = releaseData.GetApplicationAsset();
            if (asset == null)
            {
                throw new ApplicationException("No asset information found on GitHub release");
            }
            versionInfo.FileName = asset.Name;
            versionInfo.DownloadUrl = asset.DownloadUrl;
            versionInfo.Length = asset.Size;

            return versionInfo;
        }
    }
}
