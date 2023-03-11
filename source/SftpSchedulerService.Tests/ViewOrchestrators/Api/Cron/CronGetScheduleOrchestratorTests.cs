using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SftpSchedulerService.Models;
using SftpSchedulerService.Models.Cron;
using SftpSchedulerService.ViewOrchestrators.Api.Cron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Cron
{
    [TestFixture]
    public class CronGetScheduleOrchestratorTests
    {
        [TestCase("")]
        [TestCase("    ")]
        [TestCase(null)]
        public void Execute_NullOrEmptySchedule_ReturnsInvalid(string schedule)
        {
            ILogger<CronGetScheduleOrchestrator> logger = Substitute.For<ILogger<CronGetScheduleOrchestrator>>();

            CronGetScheduleOrchestrator orchestrator = new CronGetScheduleOrchestrator(logger);
            var result = orchestrator.Execute(schedule).GetAwaiter().GetResult() as OkObjectResult;
            Assert.IsNotNull(result);
            var cronResult = result.Value as CronResult;
            Assert.IsNotNull(cronResult);
            Assert.IsFalse(cronResult.IsValid);
            Assert.That(cronResult.ErrorMessage, Is.EqualTo("No cron schedule supplied"));
        }

        [Test]
        public void Execute_InvalidSchedule_ReturnsInvalid()
        {
            ILogger<CronGetScheduleOrchestrator> logger = Substitute.For<ILogger<CronGetScheduleOrchestrator>>();

            CronGetScheduleOrchestrator orchestrator = new CronGetScheduleOrchestrator(logger);
            var result = orchestrator.Execute("gobbledygook").GetAwaiter().GetResult() as OkObjectResult;
            Assert.IsNotNull(result);
            var cronResult = result.Value as CronResult;
            Assert.IsNotNull(cronResult);
            Assert.IsFalse(cronResult.IsValid);
            Assert.That(cronResult.ErrorMessage, Is.EqualTo("Invalid cron schedule"));
        }

        // examples here: https://github.com/bradymholt/cron-expression-descriptor
        [TestCase("* * * * *", "Every minute")]
        [TestCase("30 11 * * 1-5", "At 11:30 AM, Monday through Friday")]
        [TestCase("23 12 * JAN *", "At 12:23 PM, only in January")]
        public void Execute_ValidSchedule_ReturnsSchedule(string schedule, string expectedResult)
        {
            ILogger<CronGetScheduleOrchestrator> logger = Substitute.For<ILogger<CronGetScheduleOrchestrator>>();

            CronGetScheduleOrchestrator orchestrator = new CronGetScheduleOrchestrator(logger);
            var result = orchestrator.Execute(schedule).GetAwaiter().GetResult() as OkObjectResult;
            Assert.IsNotNull(result);
            var cronResult = result.Value as CronResult;
            Assert.IsNotNull(cronResult);
            Assert.IsTrue(cronResult.IsValid);
            Assert.IsNull(cronResult.ErrorMessage);
            Assert.That(cronResult.ScheduleInWords, Is.EqualTo(expectedResult));
        }

    }
}
