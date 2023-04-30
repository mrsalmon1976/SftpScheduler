using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

namespace SftpSchedulerService.Tests
{
    internal static class HttpClientTestFactory
    {
        public const string JwtValidAudience = "http://localhost:4200";

        public const string JwtValidIssuer = "https://localhost:5001";

        public const string JwtSecret = "ZzpXxkkCnbfNstnQ3R1g1VO381iUatKn";

        public static HttpClient CreateHttpClient(Action<IServiceCollection>? configureServices = null, bool allowAutoRedirect = false)
        {
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    // add a config setting so the Startup can be aware we're running in automated test mode
                    builder.UseSetting("AutomatedTestContext", "TRUE");

                    // overrides default application DI if something has been set
                    if (configureServices != null)
                    {
                        builder.ConfigureServices(configureServices);
                    }
                });
                factory.ClientOptions.AllowAutoRedirect = allowAutoRedirect;
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("SftpScheduler-Test", "1");
            return client;
        }

        public static HttpClient CreateAuthenticatedHttpClient(string[] roles, Action<IServiceCollection>? configureServices = null, bool allowAutoRedirect = false)
        {
            HttpClient httpClient = CreateHttpClient(configureServices, allowAutoRedirect);

            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "test"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            foreach (string role in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));


            var jwtSecurityToken = new JwtSecurityToken(
                issuer: JwtValidIssuer,
                audience: JwtValidAudience,
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
