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
using SystemWrapper.IO;
using SftpScheduler.BLL.Commands.Transfer;

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

    builder.Services.AddTransient<IDbMigrator, SQLiteDbMigrator>();
    builder.Services.AddTransient<IdentityInitialiser>();

    builder.Services.AddTransient<HostValidator>();
    builder.Services.AddTransient<IJobValidator, JobValidator>();

    builder.Services.AddTransient<ICreateHostCommand, CreateHostCommand>();
    builder.Services.AddTransient<CreateJobCommand>();
    builder.Services.AddTransient<ICreateJobResultCommand, CreateJobResultCommand>();
    builder.Services.AddTransient<CreateUserCommand>();
    builder.Services.AddTransient<IDeleteJobCommand, DeleteJobCommand>();
    builder.Services.AddTransient<IUpdateJobResultCompleteCommand, UpdateJobResultCompleteCommand>();

    builder.Services.AddTransient<ITransferCommandFactory, TransferCommandFactory>();

    builder.Services.AddTransient<HostRepository>();
    builder.Services.AddTransient<JobRepository>();

    builder.Services.AddQuartzScheduler(appSettings);

    builder.Services.AddTransient<CronGetScheduleOrchestrator>();
    builder.Services.AddTransient<HostCreateOrchestrator>();
    builder.Services.AddTransient<HostFetchAllOrchestrator>();
    builder.Services.AddTransient<JobCreateOrchestrator>();
    builder.Services.AddTransient<JobDeleteOneOrchestrator>();
    builder.Services.AddTransient<JobFetchAllOrchestrator>();
    builder.Services.AddTransient<JobFetchOneOrchestrator>();
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
