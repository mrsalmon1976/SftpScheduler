using Microsoft.AspNetCore.Mvc.Testing;
using SftpSchedulerService.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace SftpSchedulerService.Tests
{
    internal static class TestFactory
    {
        private static AppSettings? _appSettings;

        public static AppSettings AppSettings
        {

            get
            {
                if (_appSettings == null)
                {
                    IConfiguration config = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .AddEnvironmentVariables()
                        .Build();
                    _appSettings = new AppSettings(config, AppDomain.CurrentDomain.BaseDirectory);
                }
                return _appSettings;
            }

        }

        public static HttpClient CreateHttpClient(bool allowAutoRedirect = false)
        {
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    //builder.ConfigureTestServices(services =>
                    //{
                    //    services.AddAuthentication("JWT_OR_COOKIE")
                    //        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    //            "JWT_OR_COOKIE", options => { });
                    //});
                    // ... Configure test services
                });
                factory.ClientOptions.AllowAutoRedirect = allowAutoRedirect;
            return factory.CreateClient();
        }

        public static HttpClient CreateAuthenticatedHttpClient(string[] roles, bool allowAutoRedirect = false)
        {
            HttpClient httpClient = CreateHttpClient(allowAutoRedirect);

            AppSettings appSettings = AppSettings;
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "test"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            foreach (string role in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.JwtSecret));


            var jwtSecurityToken = new JwtSecurityToken(
                issuer: appSettings.JwtValidIssuer,
                audience: appSettings.JwtValidAudience,
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            string token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return httpClient;
        }



    }
}
