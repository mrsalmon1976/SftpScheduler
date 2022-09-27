using Microsoft.Extensions.Configuration;
using SftpScheduler.BLL.Data;
using SftpSchedulerService;
using SftpSchedulerService.BootStrapping;
using SftpSchedulerService.Config;
using SftpSchedulerService.Utilities;
using System.Net;

var webApplicationOptions = new WebApplicationOptions() { 
    ContentRootPath = AppContext.BaseDirectory, 
    Args = args, 
    ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName
};
var builder = WebApplication.CreateBuilder(webApplicationOptions);

// add services
builder.Services.AddHostedService<Worker>();
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddIdentity(builder.Configuration);

// initialise app settings
AppSettings appSettings = new AppSettings(builder.Configuration, webApplicationOptions.ContentRootPath);
builder.Services.AddSingleton<AppSettings>(appSettings);
builder.Services.AddSingleton<IDbContextFactory>(new DbContextFactory(appSettings.DbPath));
builder.Services.AddSingleton<ResourceUtils>();

// set up 
var app = builder.Build();
var config = builder.Configuration;
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
app.InitialiseDatabase(serviceProvider.GetService<IDbContextFactory>(), serviceProvider.GetService<ResourceUtils>());
app.Run();

