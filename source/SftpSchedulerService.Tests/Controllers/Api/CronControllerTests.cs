using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Quartz.Impl.Triggers;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.Caching;
using SftpSchedulerService.ViewOrchestrators.Api.Cron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Tests.Controllers.Api
{
    [TestFixture]
    public class CronControllerTests
    {
        const string DefaultSchedule = "0 0/1 * ? * * *";


        [Test]
        public void  Get_ValidateExecution()
        {
            string[] roles = { UserRoles.Admin };
            ICronGetScheduleOrchestrator orchestrator = Substitute.For<ICronGetScheduleOrchestrator>();
            Action<IServiceCollection> configureServices = CreateConfiguration(orchestrator, DefaultSchedule);

            // execute
            using (var client = HttpClientTestFactory.CreateAuthenticatedHttpClient(roles, configureServices))
            {
                var url = GetUrl(DefaultSchedule);
                var response = client.GetAsync(url).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
            }

            // assert
            orchestrator.Received(1).Execute(DefaultSchedule);
        }

        [Test]
        public void Get_Unauthenticated_ReturnsUnauthorised()
        {
            string[] roles = { UserRoles.Admin };
            ICronGetScheduleOrchestrator orchestrator = Substitute.For<ICronGetScheduleOrchestrator>();
            Action<IServiceCollection> configureServices = CreateConfiguration(orchestrator, DefaultSchedule);

            // execute
            using (var client = HttpClientTestFactory.CreateHttpClient(configureServices))
            {
                var url = GetUrl(DefaultSchedule);
                var response = client.GetAsync(url).GetAwaiter().GetResult();

                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

            }

        }

        [TestCase(UserRoles.Admin, UserRoles.User)]
        public void Get_RoleHasPermission_ReturnsOk(params string[] authorisedRoles)
        {
            foreach (string role in UserRoles.AllRoles) 
            {
                ICronGetScheduleOrchestrator orchestrator = Substitute.For<ICronGetScheduleOrchestrator>();
                Action<IServiceCollection> configureServices = CreateConfiguration(orchestrator, DefaultSchedule);
                string[] roles = { role };

                using (var client = HttpClientTestFactory.CreateAuthenticatedHttpClient(roles, configureServices))
                {
                    var url = GetUrl(DefaultSchedule);
                    var response = client.GetAsync(url).GetAwaiter().GetResult();

                    // check if the role was allowed
                    if (authorisedRoles.Contains(role))
                    {
                        response.EnsureSuccessStatusCode();
                        orchestrator.Received(1).Execute(DefaultSchedule);
                    }
                    else
                    {
                        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
                        orchestrator.DidNotReceive().Execute(Arg.Any<string>());
                    }


                }

            }
        }


        private static string GetUrl(string schedule)
        {
            return $"/api/cron?schedule={WebUtility.UrlEncode(schedule)}";
        }

        private static Action<IServiceCollection> CreateConfiguration(ICronGetScheduleOrchestrator orchestrator, string schedule)
        {
            orchestrator.Execute(schedule).Returns(new OkObjectResult("ok!"));
            Action<IServiceCollection> configureServices = (cfg) =>
            {
                cfg.AddSingleton<ICronGetScheduleOrchestrator>(orchestrator);
            };
            return configureServices;
        }
    }
}
