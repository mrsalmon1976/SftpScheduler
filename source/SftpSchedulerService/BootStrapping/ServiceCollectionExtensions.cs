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
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Validators;
using SftpSchedulerService.Config;
using SftpSchedulerService.ViewOrchestrators.Api.Cron;
using SftpSchedulerService.ViewOrchestrators.Api.Host;
using SftpSchedulerService.ViewOrchestrators.Api.Job;
using SftpSchedulerService.ViewOrchestrators.Api.JobLog;
using SftpSchedulerService.ViewOrchestrators.Api.Login;
using SftpSchedulerService.ViewOrchestrators.Api.User;
using System.Text;

namespace SftpSchedulerService.BootStrapping
{
    public static class ServiceCollectionExtensions
    {
        private const string CustomAuthenticationScheme = "JWT_OR_COOKIE";


        public static void AddCommands(this IServiceCollection services)
        {
            services.AddScoped<ICreateHostCommand, CreateHostCommand>();
            services.AddScoped<IDeleteHostCommand, DeleteHostCommand>();
            services.AddScoped<IUpdateHostCommand, UpdateHostCommand>();

            services.AddScoped<CreateJobCommand>();
            services.AddScoped<IExecuteJobCommand, ExecuteJobCommand>();
            services.AddScoped<IUpdateJobCommand, UpdateJobCommand>();
            services.AddScoped<ICreateJobLogCommand, CreateJobLogCommand>();

            services.AddScoped<CreateUserCommand>();
            services.AddScoped<IDeleteJobCommand, DeleteJobCommand>();

            services.AddScoped<IUpdateJobLogCompleteCommand, UpdateJobLogCompleteCommand>();

            services.AddScoped<ITransferCommand, TransferCommand>();
        }

        public static void AddIdentity(this IServiceCollection services, AppSettings appSettings)
        {

            services.AddDbContext<SftpSchedulerIdentityDbContext>(options => options.UseSqlite(appSettings.DbConnectionString));
            services.AddIdentity<UserEntity, IdentityRole>()
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
            services.AddScoped<ICronGetScheduleOrchestrator, CronGetScheduleOrchestrator>();

            services.AddScoped<IHostCreateOrchestrator, HostCreateOrchestrator>();
            services.AddScoped<IHostDeleteOneOrchestrator, HostDeleteOneOrchestrator>();
            services.AddScoped<IHostFetchAllOrchestrator, HostFetchAllOrchestrator>();
            services.AddScoped<IHostFetchOneOrchestrator, HostFetchOneOrchestrator>();
            services.AddScoped<IHostFingerprintScanOrchestrator, HostFingerprintScanOrchestrator>();
            services.AddScoped<IHostUpdateOrchestrator, HostUpdateOrchestrator>();

            services.AddScoped<IJobCreateOrchestrator, JobCreateOrchestrator>();
            services.AddScoped<IJobExecuteOrchestrator, JobExecuteOrchestrator>();
            services.AddScoped<IJobUpdateOrchestrator, JobUpdateOrchestrator>();
            services.AddScoped<IJobDeleteOneOrchestrator, JobDeleteOneOrchestrator>();
            services.AddScoped<IJobFetchAllOrchestrator, JobFetchAllOrchestrator>();
            services.AddScoped<IJobFetchOneOrchestrator, JobFetchOneOrchestrator>();
            services.AddScoped<IJobLogFetchAllOrchestrator, JobLogFetchAllOrchestrator>();

            services.AddScoped<IJobNotificationFetchAllOrchestrator, JobNotificationFetchAllOrchestrator>();

            services.AddScoped<ILoginPostOrchestrator, LoginPostOrchestrator>();

            services.AddScoped<IUserFetchAllOrchestrator, UserFetchAllOrchestrator>();
        }

        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<HostRepository>();
            services.AddScoped<JobRepository>();
            services.AddScoped<JobLogRepository>();
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
                options.AwaitApplicationStarted = true;
            });

            if (appSettings.IsAutomatedTestContext)
            {
                Quartz.Logging.LogProvider.IsDisabled = true;
            }


        }

        public static void AddValidators(this IServiceCollection services)
        {
            services.AddTransient<HostValidator>();
            services.AddTransient<IJobValidator, JobValidator>();
        }
    }
}
