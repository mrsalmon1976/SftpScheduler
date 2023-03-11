using SftpScheduler.BLL.Data;
using SftpSchedulerService;
using SftpScheduler.BLL.Utility;
using SftpSchedulerService.BootStrapping;
using SftpSchedulerService.Config;
using SftpSchedulerService.Utilities;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Validators;
using SftpScheduler.BLL.Repositories;
using NLog.Web;
using NLog;
using Microsoft.AspNetCore.Identity;
using SftpScheduler.BLL.Command.User;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.ViewOrchestrators.Api.Host;
using SftpSchedulerService.ViewOrchestrators.Api.Login;
using SftpSchedulerService.ViewOrchestrators.Api.Cron;

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

    builder.Services.AddTransient<IDbMigrator, SQLiteDbMigrator>();
    builder.Services.AddTransient<IdentityInitialiser>();

    builder.Services.AddTransient<HostValidator>();

    builder.Services.AddTransient<ICreateHostCommand, CreateHostCommand>();
    builder.Services.AddTransient<CreateUserCommand>();

    builder.Services.AddTransient<HostRepository>();

    builder.Services.AddTransient<CronGetScheduleOrchestrator>();
    builder.Services.AddTransient<HostCreateOrchestrator>();
    builder.Services.AddTransient<HostFetchAllOrchestrator>();
    builder.Services.AddTransient<LoginPostOrchestrator>();

    // set up 
    var app = builder.Build();
    //app.Environment.ApplicationName = 
    //var config = builder.Configuration;
    var serviceProvider = app.Services;


    //app.UseStatusCodePages(async context => {
    //    var response = context.HttpContext.Response;

    //    if (response.StatusCode == (int)HttpStatusCode.Unauthorized)
    //    {
    //        response.Redirect("/auth/login");
    //    }
    //});
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseStaticFiles();
    app.MapControllerRoute(name: "default", pattern: "{controller=Dashboard}/{action=Index}/{id?}");
    app.InitialiseDatabase();
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
