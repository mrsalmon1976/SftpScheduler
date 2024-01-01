using Microsoft.Extensions.DependencyInjection;
using NLog;
using SftpSchedulerServiceUpdater;

var logger = LogManager.Setup().LoadConfigurationFromFile().GetCurrentClassLogger();
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

