using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.Config;
using System.Text;

namespace SftpSchedulerService.BootStrapping
{
    public static class ServiceCollectionExtensions
    {
        private const string CustomAuthenticationScheme = "JWT_OR_COOKIE";

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
    }
}
