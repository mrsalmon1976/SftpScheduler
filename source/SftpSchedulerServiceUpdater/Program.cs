using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using SftpSchedulerServiceUpdater;

var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
var host = BootStrapper.CreateHostBuilder(args).Build();

try
{
    logger.Info("SftpSchedulerService Update start");
    UpdateOrchestrator orchestrator = host.Services.GetRequiredService<UpdateOrchestrator>();
    orchestrator.Run().GetAwaiter().GetResult();
}
catch (Exception ex)
{
    logger.Error(ex);
}
finally
{
    logger.Info("SftpSchedulerService Update complete");
}

