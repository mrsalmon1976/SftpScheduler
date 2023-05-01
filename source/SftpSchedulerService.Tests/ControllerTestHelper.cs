using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.ViewOrchestrators;
using SftpSchedulerService.ViewOrchestrators.Api.Cron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace SftpSchedulerService.Tests
{
    internal static class ControllerTestHelper
    {
        /// <summary>
        /// Configures services for a specific view orchestrator
        /// </summary>
        /// <typeparam name="TOrchestrator"></typeparam>
        /// <param name="orchestrator"></param>
        /// <returns></returns>
        public static Action<IServiceCollection> CreateConfiguration<TOrchestrator>(TOrchestrator orchestrator) 
            where TOrchestrator : class, IViewOrchestrator
        {
            Action<IServiceCollection> configureServices = (cfg) =>
            {
                // this is the ovverride
                cfg.AddSingleton<TOrchestrator>(orchestrator);
            };
            return configureServices;
        }

        public static void ExecuteSuccess(string url, string[] roles, Action<IServiceCollection> configureServices)
        {
            using (var client = HttpClientTestFactory.CreateAuthenticatedHttpClient(roles, configureServices))
            {
                var response = client.GetAsync(url).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
            }
        }

        public static void ExecuteUnauthorised(string url, Action<IServiceCollection> configureServices)
        {
            using (var client = HttpClientTestFactory.CreateHttpClient(configureServices))
            {
                var response = client.GetAsync(url).GetAwaiter().GetResult();
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            }
        }

        public static void CheckAllRoles<TOrchestrator>(TOrchestrator orchestrator, string url, params string[] authorisedRoles)
            where TOrchestrator : class, IViewOrchestrator
        {
            foreach (string role in UserRoles.AllRoles)
            {
                Action<IServiceCollection> configureServices = CreateConfiguration<TOrchestrator>(orchestrator);
                string[] roles = { role };

                using (var client = HttpClientTestFactory.CreateAuthenticatedHttpClient(roles, configureServices))
                {
                    var response = client.GetAsync(url).GetAwaiter().GetResult();

                    // check if the role was allowed
                    if (authorisedRoles.Contains(role))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    else
                    {
                        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
                    }
                }
            }
        }
    }
}
