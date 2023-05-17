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
using SftpScheduler.BLL.IO;
using System.Diagnostics;

var logger = LogManager.Setup().LoadConfigurationFromFile().GetCurrentClassLogger();

var webApplicationOptions = new WebApplicationOptions()
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
    //WebRootPath = AppContext.BaseDirectory
    //ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName
};
var builder = WebApplication.CreateBuilder(webApplicationOptions);
builder.Host.UseWindowsService();

try
{
    logger.Info("SftpScheduler starting up");
    bool isAutomatedTestContext = (builder.Configuration["AutomatedTestContext"] == "TRUE");
    logger.Info("Context: " + (isAutomatedTestContext ? "Automated tests" : "Application"));

    // initialise app settings
    logger.Debug("Application base directory: {baseDirectory}", AppContext.BaseDirectory);
    AppSettings appSettings = new AppSettings(builder.Configuration, AppContext.BaseDirectory, isAutomatedTestContext);

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
    builder.Services.AddScoped<IDbContext>((sp) => new SQLiteDbContext(appSettings.DbPath));
    builder.Services.AddSingleton<ResourceUtils>();

    builder.Services.AddScoped<IDirectoryUtility, DirectoryUtility>();
    builder.Services.AddScoped<IFileUtility, FileUtility>();
    builder.Services.AddScoped<IFileTransferService, FileTransferService>();
    builder.Services.AddScoped<IPasswordProvider>((sp) => new PasswordProvider(appSettings.SecretKey));
    builder.Services.AddScoped<ISessionWrapperFactory, SessionWrapperFactory>();
    builder.Services.AddScoped<ICacheProvider,  CacheProvider>();


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
    var serviceProvider = app.Services;

    app.UseAuthentication();
    app.UseAuthorization();

    if (!appSettings.IsAutomatedTestContext && !Debugger.IsAttached)
    {
        app.Urls.Add($"http://0.0.0.0:8743");
        //app.Urls.Add($"https://0.0.0.0:8744");
    }
    app.UseStaticFiles();

    app.MapControllerRoute(name: "default", pattern: "{controller=Dashboard}/{action=Index}/{id?}");
    if (!appSettings.IsAutomatedTestContext) 
    {
        app.InitialiseDatabase(appSettings);
    }

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
