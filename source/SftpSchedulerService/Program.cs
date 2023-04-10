using SftpScheduler.BLL.Data;
using SftpSchedulerService;
using SftpScheduler.BLL.Utility;
using SftpSchedulerService.BootStrapping;
using SftpSchedulerService.Config;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Validators;
using SftpScheduler.BLL.Repositories;
using NLog.Web;
using NLog;
using SftpScheduler.BLL.Commands.User;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.ViewOrchestrators.Api.Host;
using SftpSchedulerService.ViewOrchestrators.Api.Login;
using SftpSchedulerService.ViewOrchestrators.Api.Cron;
using SftpSchedulerService.ViewOrchestrators.Api.Job;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Commands.Transfer;
using SftpScheduler.BLL.Utility.IO;
using SftpSchedulerService.ViewOrchestrators.Api.JobLog;

//var webApplicationOptions = new WebApplicationOptions() { 
//    ContentRootPath = AppContext.BaseDirectory, 
//    Args = args, 
//    ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName
//};
var logger = NLog.LogManager.Setup().LoadConfigurationFromFile().GetCurrentClassLogger();
var builder = WebApplication.CreateBuilder(args);       // NOTE! Without args, integration tests don't work!?
//builder.Environment.ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
//builder.Environment.ContentRootPath = AppContext.BaseDirectory;
//builder.Configuration.con

try
{
    logger.Info("SftpScheduler starting up");
    // initialise app settings
    AppSettings appSettings = new AppSettings(builder.Configuration, AppContext.BaseDirectory);

    // logging
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    // add services
    builder.Services.AddHostedService<Worker>();
    builder.Services.AddControllers();
    builder.Services.AddRazorPages();
    builder.Services.AddAutoMapper(AutoMapperBootstrapper.Configure);

    // custom
    builder.Services.AddIdentity(appSettings);

    builder.Services.AddSingleton<AppSettings>(appSettings);
    builder.Services.AddSingleton<IDbContextFactory>(new DbContextFactory(appSettings.DbPath));
    builder.Services.AddSingleton<ResourceUtils>();

    builder.Services.AddTransient<IDirectoryWrap, DirectoryWrap>();
    builder.Services.AddTransient<IFileWrap, FileWrap>();
    builder.Services.AddTransient<IFileTransferService, FileTransferService>();
    builder.Services.AddTransient<ISessionWrapperFactory, SessionWrapperFactory>();


    builder.Services.AddTransient<IDbMigrator, SQLiteDbMigrator>();
    builder.Services.AddTransient<IdentityInitialiser>();

    // custom stuff
    builder.Services.AddRepositories();
    builder.Services.AddValidators();
    builder.Services.AddCommands();
    builder.Services.AddControllerOrchestrators();

    builder.Services.AddQuartzScheduler(appSettings);

    // set up 
    var app = builder.Build();
    //app.Environment.ApplicationName = 
    //var config = builder.Configuration;
    var serviceProvider = app.Services;

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseStaticFiles();
    app.MapControllerRoute(name: "default", pattern: "{controller=Dashboard}/{action=Index}/{id?}");
    app.InitialiseDatabase(appSettings);
    app.Run();
}
catch (Exception ex)
{
    logger.Fatal(ex, ex.Message);
}
finally
{
    logger.Info("SftpScheduler shutting down");
    NLog.LogManager.Shutdown();
}
public partial class Program 
{
}
