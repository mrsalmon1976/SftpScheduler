using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Quartz;
using Quartz.Impl;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Commands.Transfer;
using SftpScheduler.BLL.Commands.User;
using SftpScheduler.BLL.Identity;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Validators;
using SftpSchedulerService.Config;
using SftpSchedulerService.ViewOrchestrators.Api.Cron;
using SftpSchedulerService.ViewOrchestrators.Api.Host;
using SftpSchedulerService.ViewOrchestrators.Api.Job;
using SftpSchedulerService.ViewOrchestrators.Api.JobLog;
using SftpSchedulerService.ViewOrchestrators.Api.Login;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Text;

namespace SftpSchedulerService.BootStrapping
{
    public static class ServiceCollectionExtensions
    {
        private const string CustomAuthenticationScheme = "JWT_OR_COOKIE";


        public static void AddCommands(this IServiceCollection services)
        {
            services.AddTransient<ICreateHostCommand, CreateHostCommand>();
            services.AddTransient<IDeleteHostCommand, DeleteHostCommand>();
            services.AddTransient<IUpdateHostCommand, UpdateHostCommand>();

            services.AddTransient<CreateJobCommand>();
            services.AddTransient<IExecuteJobCommand, ExecuteJobCommand>();
            services.AddTransient<IUpdateJobCommand, UpdateJobCommand>();
            services.AddTransient<ICreateJobLogCommand, CreateJobLogCommand>();

            services.AddTransient<CreateUserCommand>();
            services.AddTransient<IDeleteJobCommand, DeleteJobCommand>();

            services.AddTransient<IUpdateJobLogCompleteCommand, UpdateJobLogCompleteCommand>();

            services.AddTransient<ITransferCommand, TransferCommand>();
        }

        public static void AddIdentity(this IServiceCollection services, AppSettings appSettings)
        {

            services.AddDbContext<SftpSchedulerIdentityDbContext>(options => options.UseSqlite(appSettings.DbConnectionString));
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<SftpSchedulerIdentityDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 5;
                options.Password.RequiredUniqueChars = 1;
            });

            // Adding Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CustomAuthenticationScheme;
                options.DefaultChallengeScheme = CustomAuthenticationScheme;
                options.DefaultScheme = CustomAuthenticationScheme;
            })
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = "/auth/login";
                    options.ExpireTimeSpan = TimeSpan.FromDays(1);
                    options.Events.OnRedirectToLogin = (ctx) =>
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                            ctx.Response.StatusCode = 401;
                        else
                            ctx.Response.Redirect(ctx.RedirectUri);

                        return Task.CompletedTask;
                    };
                })
                .AddJwtBearer("Bearer", options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidAudience = appSettings.JwtValidAudience,
                        ValidIssuer = appSettings.JwtValidIssuer,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.JwtSecret))
                    };
                })
                .AddPolicyScheme(CustomAuthenticationScheme, CustomAuthenticationScheme, options =>
                {
                    // runs on each request
                    options.ForwardDefaultSelector = context =>
                    {
                        // filter by auth type
                        string authorization = context.Request.Headers[HeaderNames.Authorization];
                        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                            return "Bearer";

                        // otherwise always check for cookie auth
                        return "Cookies";
                    };
                });

        }

        public static void AddControllerOrchestrators(this IServiceCollection services)
        {
            services.AddTransient<CronGetScheduleOrchestrator>();

            services.AddTransient<HostCreateOrchestrator>();
            services.AddTransient<HostDeleteOneOrchestrator>();
            services.AddTransient<HostFetchAllOrchestrator>();
            services.AddTransient<HostFetchOneOrchestrator>();
            services.AddTransient<HostFingerprintScanOrchestrator>();
            services.AddTransient<HostUpdateOrchestrator>();

            services.AddTransient<JobCreateOrchestrator>();
            services.AddTransient<JobExecuteOrchestrator>();
            services.AddTransient<JobUpdateOrchestrator>();
            services.AddTransient<JobDeleteOneOrchestrator>();
            services.AddTransient<JobFetchAllOrchestrator>();
            services.AddTransient<JobFetchOneOrchestrator>();
            services.AddTransient<JobLogFetchAllOrchestrator>();

            services.AddTransient<JobNotificationFetchAllOrchestrator>();

            services.AddTransient<LoginPostOrchestrator>();
        }

        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<HostRepository>();
            services.AddTransient<JobRepository>();
            services.AddTransient<JobLogRepository>();
        }

        public static void AddQuartzScheduler(this IServiceCollection services, AppSettings appSettings)
        {
            services.AddQuartz(q =>
            {
                q.SchedulerId = "SftpScheduler";
                q.SchedulerName = "SftpScheduler";
                q.UseMicrosoftDependencyInjectionJobFactory();
                q.UseDefaultThreadPool(x => x.MaxConcurrency = appSettings.MaxConcurrentJobs);
                q.UsePersistentStore(x =>
                {
                    x.UseProperties = true;
                    x.Properties.Add("quartz.jobStore.txIsolationLevelSerializable", "true");
                    x.UseSQLite(appSettings.DbConnectionStringQuartz);
                    x.UseJsonSerializer();
                });
            });

            services.AddTransient<TransferJob>();

            services.AddQuartzHostedService(options =>
            {
                // FTP jobs could take forever....we just need to cut our losses if we are shutting down
                options.WaitForJobsToComplete = false;
            });

            //var properties = new NameValueCollection();

            //StdSchedulerFactory schedulerFactory = SchedulerBuilder.Create(properties)
            //    .UseDefaultThreadPool(x => x.MaxConcurrency = 5)
            //    // this is the default 
            //    // .WithMisfireThreshold(TimeSpan.FromSeconds(60))
            //    .UsePersistentStore(x =>
            //    {
            //        // force job data map values to be considered as strings
            //        // prevents nasty surprises if object is accidentally serialized and then 
            //        // serialization format breaks, defaults to false
            //        x.UseProperties = true;
            //        x.UseClustering();
            //        x.UseSQLite(appSettings.DbConnectionString)
            //    })
            //    .Build();

            //IScheduler scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
            //scheduler.Start().GetAwaiter().GetResult();

            //
        }

        public static void AddValidators(this IServiceCollection services)
        {
            services.AddTransient<HostValidator>();
            services.AddTransient<IJobValidator, JobValidator>();
        }
    }
}
