using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Quartz;
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
using SftpSchedulerService.ViewOrchestrators.Api.Auth;
using SftpSchedulerService.ViewOrchestrators.Api.Update;
using SftpSchedulerService.ViewOrchestrators.Api.User;
using System.Text;
using SftpSchedulerService.ViewOrchestrators.Api.Dashboard;
using SftpSchedulerService.ViewOrchestrators.Api.Settings;
using SftpScheduler.BLL.Commands.Notification;
using SftpScheduler.BLL.Commands.Setting;

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

            services.AddScoped<ICreateJobCommand, CreateJobCommand>();
            services.AddScoped<IExecuteJobCommand, ExecuteJobCommand>();
            services.AddScoped<IUpdateJobCommand, UpdateJobCommand>();
            services.AddScoped<ICreateJobLogCommand, CreateJobLogCommand>();
            services.AddScoped<IDeleteJobCommand, DeleteJobCommand>();

            services.AddScoped<ICreateUserCommand, CreateUserCommand>();
            services.AddScoped<IUpdateUserCommand, UpdateUserCommand>();

            services.AddScoped<IChangePasswordCommand, ChangePasswordCommand>();

            services.AddScoped<IUpdateJobLogCompleteCommand, UpdateJobLogCompleteCommand>();

            services.AddScoped<ITransferCommand, TransferCommand>();

            services.AddScoped<IUpsertGlobalUserSettingCommand, UpsertGlobalUserSettingCommand>();


			services.AddScoped<IUpsertDigestCommand, UpsertDigestCommand>();
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

        public static void AddViewOrchestrators(this IServiceCollection services)
        {
            services.AddScoped<ICronGetScheduleOrchestrator, CronGetScheduleOrchestrator>();

            services.AddScoped<IDashboardFetchAllOrchestrator, DashboardFetchAllOrchestrator>();

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

            services.AddScoped<IUpdateCheckOrchestrator, UpdateCheckOrchestrator>();
            services.AddScoped<IUpdateInstallOrchestrator, UpdateInstallOrchestrator>();

            services.AddScoped<IChangePasswordPostOrchestrator, ChangePasswordPostOrchestrator>();

            services.AddScoped<ISettingsFetchAllOrchestrator, SettingsFetchAllOrchestrator>();
            services.AddScoped<ISettingsUpdateOrchestrator, SettingsUpdateOrchestrator>();
            services.AddScoped<ISettingsEmailTestOrchestrator, SettingsEmailTestOrchestrator>();

            services.AddScoped<IUserCreateOrchestrator, UserCreateOrchestrator>();
            services.AddScoped<IUserFetchAllOrchestrator, UserFetchAllOrchestrator>();
            services.AddScoped<IUserFetchOneOrchestrator, UserFetchOneOrchestrator>();
            services.AddScoped<IUserUpdateOrchestrator, UserUpdateOrchestrator>();
        }

        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<GlobalUserSettingRepository>();
			services.AddScoped<HostRepository>();
			services.AddScoped<JobRepository>();
            services.AddScoped<JobLogRepository>();
			services.AddScoped<IUserRepository, UserRepository>();
		}

        public static void AddQuartzScheduler(this IServiceCollection services, AppSettings appSettings, int maxConcurrentJobs, bool isUnitTestContext)
        {
            services.AddQuartz(q =>
            {
                q.SchedulerId = "SftpScheduler";
                q.SchedulerName = "SftpScheduler";
                q.UseMicrosoftDependencyInjectionJobFactory();
                q.UseDefaultThreadPool(x => x.MaxConcurrency = maxConcurrentJobs);
                if (isUnitTestContext)
                {
                    q.UseInMemoryStore();
                    Quartz.Logging.LogProvider.IsDisabled = true;
                }
                else
                {
                    q.UsePersistentStore(x =>
                    {
                        x.UseProperties = true;
                        x.Properties.Add("quartz.jobStore.txIsolationLevelSerializable", "true");
                        x.UseSQLite(appSettings.DbConnectionStringQuartz);
                        x.UseJsonSerializer();
                    });
                }
            });

            services.AddTransient<TransferJob>();

            services.AddQuartzHostedService(options =>
            {
                // FTP jobs could take forever....we just need to cut our losses if we are shutting down
                options.WaitForJobsToComplete = false;
                options.AwaitApplicationStarted = true;
                options.StartDelay = TimeSpan.FromSeconds(5);
            });

        }

        public static void AddValidators(this IServiceCollection services)
        {
            services.AddTransient<IEmailValidator, EmailValidator>();
            services.AddTransient<IHostValidator, HostValidator>();
            services.AddTransient<IJobValidator, JobValidator>();
            services.AddTransient<ISmtpHostValidator, SmtpHostValidator>();
            services.AddTransient<IUserValidator, UserValidator>();
        }
    }
}
