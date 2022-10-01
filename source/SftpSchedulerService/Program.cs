using SftpScheduler.BLL.Data;
using SftpSchedulerService;
using SftpScheduler.BLL.Utility;
using SftpSchedulerService.BootStrapping;
using SftpSchedulerService.Config;
using SftpSchedulerService.Utilities;

//var webApplicationOptions = new WebApplicationOptions() { 
//    ContentRootPath = AppContext.BaseDirectory, 
//    Args = args, 
//    ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName
//};
var builder = WebApplication.CreateBuilder(args);       // NOTE! Without args, integration tests don't work!?
//builder.Environment.ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
//builder.Environment.ContentRootPath = AppContext.BaseDirectory;
//builder.Configuration.con

// initialise app settings
AppSettings appSettings = new AppSettings(builder.Configuration, AppContext.BaseDirectory);

// add services
builder.Services.AddHostedService<Worker>();
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddIdentity(appSettings);

builder.Services.AddSingleton<AppSettings>(appSettings);
builder.Services.AddSingleton<IDbContextFactory>(new DbContextFactory(appSettings.DbPath));
builder.Services.AddSingleton<ResourceUtils>();

builder.Services.AddTransient<IDbMigrator, SQLiteDbMigrator>();

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
#pragma warning disable CS8604 // Possible null reference argument.
app.InitialiseDatabase(serviceProvider.GetService<IDbMigrator>());
#pragma warning restore CS8604 // Possible null reference argument.
app.Run();


public partial class Program { }
