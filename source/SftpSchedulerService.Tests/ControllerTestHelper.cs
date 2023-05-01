using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NSubstitute;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.ViewOrchestrators;
using SftpSchedulerService.ViewOrchestrators.Api.Cron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;

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

        public static void ExecuteSuccess(string url, HttpMethod method, object? content, string[] roles, Action<IServiceCollection> configureServices)
        {
            HttpResponseMessage? response = null;
            using (var client = HttpClientTestFactory.CreateAuthenticatedHttpClient(roles, configureServices))
            {
                if (method == HttpMethod.Get)
                {
                    response = client.GetAsync(url).GetAwaiter().GetResult();
                }
                else if (method == HttpMethod.Post)
                {
                    response = client.PostAsync(url, CreateJsonContent(content)).GetAwaiter().GetResult();
                }
                else if (method == HttpMethod.Delete)
                {
                    response = client.DeleteAsync(url).GetAwaiter().GetResult();
                }
                else
                {
                    throw new NotImplementedException($"Method {method} not supported");
                }
            }
            response.EnsureSuccessStatusCode();
        }

        public static void ExecuteUnauthorised(string url, HttpMethod method, object? content, Action<IServiceCollection> configureServices)
        {
            HttpResponseMessage? response = null;
            using (var client = HttpClientTestFactory.CreateHttpClient(configureServices))
            {
                if (method == HttpMethod.Get)
                {
                    response = client.GetAsync(url).GetAwaiter().GetResult();
                }
                else if (method == HttpMethod.Post)
                {
                    response = client.PostAsync(url, CreateJsonContent(content)).GetAwaiter().GetResult();
                }
                else if (method == HttpMethod.Delete)
                {
                    response = client.DeleteAsync(url).GetAwaiter().GetResult();
                }
                else
                {
                    throw new NotImplementedException($"Method {method} not supported");
                }
            }
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        public static void CheckAllRoles<TOrchestrator>(TOrchestrator orchestrator, string url, HttpMethod method, object? content, params string[] authorisedRoles)
            where TOrchestrator : class, IViewOrchestrator
        {
            foreach (string role in UserRoles.AllRoles)
            {
                Action<IServiceCollection> configureServices = CreateConfiguration<TOrchestrator>(orchestrator);
                string[] roles = { role };

                using (var client = HttpClientTestFactory.CreateAuthenticatedHttpClient(roles, configureServices))
                {
                    HttpResponseMessage? response = null;

                    if (method == HttpMethod.Get)
                    {
                        response = client.GetAsync(url).GetAwaiter().GetResult();
                    }
                    else if (method == HttpMethod.Post)
                    {
                        response = client.PostAsync(url, CreateJsonContent(content)).GetAwaiter().GetResult();
                    }
                    else if (method == HttpMethod.Delete)
                    {
                        response = client.DeleteAsync(url).GetAwaiter().GetResult();
                    }
                    else
                    {
                        throw new NotImplementedException($"Method {method} not supported");
                    }

                    // check if the role was allowed
                    if (authorisedRoles.Contains(role))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    else
                    {
                        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
                    }
                }
            }
        }

        private static StringContent CreateJsonContent(object? content)
        {
            string? json = null;
            if (content != null)
            {
                json = JsonConvert.SerializeObject(content);
            }

            return new StringContent(json ?? String.Empty, Encoding.UTF8, "application/json");
        }
    }
}
