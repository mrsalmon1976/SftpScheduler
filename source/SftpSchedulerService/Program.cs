using SftpScheduler.BLL.Data;
using SftpSchedulerService;
using SftpScheduler.BLL.Utility;
using SftpSchedulerService.BootStrapping;
using SftpSchedulerService.Config;
using NLog.Web;
using NLog;
using SftpScheduler.BLL.Identity;
using SftpScheduler.BLL.Commands.Transfer;
using SftpScheduler.BLL.Security;
using SftpSchedulerService.Caching;
using SftpScheduler.Common.IO;
using System.Diagnostics;
using SftpScheduler.Common.Services;
using SftpScheduler.Common.Diagnostics;
using SftpSchedulerService.Utilities;
using SftpScheduler.BLL.Net;
using SftpScheduler.BLL.Commands.Notification;
using SftpScheduler.BLL.Config;
using SftpSchedulerService.Workers;

Logger? logger = null;

var webApplicationOptions = new WebApplicationOptions()
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
};
var builder = WebApplication.CreateBuilder(webApplicationOptions);
builder.Host.UseWindowsService();

try
{
    bool isUnitTestContext = AppUtils.IsUnitTestContext;

    // logging - do this first!
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();
    logger = LogManager.Setup().LoadConfigurationFromFile().GetCurrentClassLogger();

    logger.Info("SftpScheduler starting up");
    logger.Debug("Context: " + (isUnitTestContext ? "Automated tests" : "Application"));

    // initialise app settings
    logger.Debug("Application base directory: {baseDirectory}", AppContext.BaseDirectory);
    AppSettings appSettings = new AppSettings(builder.Configuration, AppContext.BaseDirectory);
    builder.Services.AddSingleton<AppSettings>(appSettings);
	builder.Services.AddScoped<IGlobalUserSettingProvider, GlobalUserSettingProvider>();

	IStartupSettingProvider startupSettingsProvider = new StartupSettingProvider(appSettings, new DirectoryUtility(), new FileUtility());
	builder.Services.AddSingleton<IStartupSettingProvider>(startupSettingsProvider);
	StartupSettings startupSettings = startupSettingsProvider.Load();


	// add services
	builder.Services.AddControllers();
    builder.Services.AddRazorPages();
    builder.Services.AddAutoMapper(AutoMapperBootstrapper.Configure);

    // custom
    builder.Services.AddIdentity(appSettings);

    builder.Services.AddSingleton<IDbContextFactory>(new DbContextFactory(appSettings.DbPath));
    builder.Services.AddScoped<IDbContext>((sp) => new SQLiteDbContext(appSettings.DbPath));
    builder.Services.AddSingleton<ResourceUtils>();
    builder.Services.AddScoped<CronBuilder>();

    builder.Services.AddScoped<IDirectoryUtility, DirectoryUtility>();
    builder.Services.AddScoped<IFileUtility, FileUtility>();
    builder.Services.AddScoped<IFileTransferService, FileTransferService>();
    builder.Services.AddScoped<IPasswordProvider, PasswordProvider>();
    builder.Services.AddScoped<IEncryptionProvider, EncryptionProvider>();
    builder.Services.AddScoped<ISessionWrapperFactory, SessionWrapperFactory>();
    builder.Services.AddScoped<ISmtpClientWrapper, SmtpClientWrapper>();
    builder.Services.AddScoped<ICacheProvider, CacheProvider>();
    builder.Services.AddSingleton<SftpScheduler.Common.Web.IHttpClientFactory, SftpScheduler.Common.Web.HttpClientFactory>();
    builder.Services.AddSingleton<IProcessWrapperFactory, ProcessWrapperFactory>();

    builder.Services.AddScoped<IApplicationVersionService, ApplicationVersionService>();
    builder.Services.AddScoped<IGitHubVersionService, GitHubVersionService>();
    builder.Services.AddScoped<IVersionComparisonService, VersionComparisonService>();

    builder.Services.AddTransient<IDbMigrator, SQLiteDbMigrator>();
    builder.Services.AddTransient<IdentityInitialiser>();


    // custom stuff
    builder.Services.AddRepositories();
    builder.Services.AddValidators();
    builder.Services.AddCommands();
    builder.Services.AddViewOrchestrators();

    builder.Services.AddQuartzScheduler(appSettings, startupSettings.MaxConcurrentJobs, isUnitTestContext);

    // background jobs
    if (!isUnitTestContext)
    {
		builder.Services.AddScoped<IUpdateCleanupWorkerService, UpdateCleanupWorkerService>();
		builder.Services.AddHostedService<UpdateCleanupWorker>();
    }

	// set up 
	var app = builder.Build();

    app.UseAuthentication();
    app.UseAuthorization();

    if (!isUnitTestContext && !Debugger.IsAttached)
    {
        app.Urls.Add($"http://0.0.0.0:{appSettings.Port}");
    }
    app.UseStaticFiles();

    app.MapControllerRoute(name: "default", pattern: "{controller=Dashboard}/{action=Index}/{id?}");
    if (!isUnitTestContext) 
    {
        app.InitialiseDatabase(appSettings);
        app.InitialiseDigestJob();
    }

    app.Run();
}
catch (Exception ex)
{
    if (logger != null)
    {
        logger.Fatal(ex, ex.Message);
    }
    else
    {
        // if we get here, our NLog setup failed so this is a last-gasp attempt
        string logPath = Path.Combine(AppContext.BaseDirectory, "errordump.log");
        File.WriteAllText(logPath, ex.Message + Environment.NewLine + ex.StackTrace);
    }
}
finally
{
    logger?.Info("SftpScheduler shutting down");
    NLog.LogManager.Shutdown();
}
public partial class Program 
{
}
