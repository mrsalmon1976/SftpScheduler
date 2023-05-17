using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using SftpScheduler.BLL.IO;
using SftpSchedulerService.AutoUpdater.Config;
using SftpSchedulerService.AutoUpdater.Services;
using SftpSchedulerService.Common.Services;
using SftpSchedulerService.Common.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.AutoUpdater
{
    internal static class BootStrapper
    {
        internal static IHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseNLog();

            return builder
                .ConfigureServices(
                    (context, services) => services
                        .AddSingleton<IVersionComparisonService, VersionComparisonService>()
                        .AddSingleton<AppSettings>()
                        .AddSingleton<UpdateLocationInfo>()
                        .AddSingleton<IApplicationVersionService, ApplicationVersionService>()
                        .AddSingleton<IDirectoryUtility, DirectoryUtility>()
                        .AddSingleton<IFileUtility, FileUtility>()
                        .AddSingleton<IInstallationService, InstallationService>()
                        .AddSingleton<IUpdateDownloadService, UpdateDownloadService>()
                        .AddSingleton<IUpdateFileService, UpdateFileService>()
                        .AddSingleton<IGitHubVersionService, GitHubVersionService>()
                        .AddSingleton<Common.Web.IHttpClientFactory, HttpClientFactory>()
                        .AddSingleton<UpdateOrchestrator, UpdateOrchestrator>()
                        );
        }
    }
}
