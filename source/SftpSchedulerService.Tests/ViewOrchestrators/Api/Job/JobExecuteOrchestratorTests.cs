using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Quartz;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Validators;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.Utilities;
using SftpSchedulerService.ViewOrchestrators.Api.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Job
{
    [TestFixture]
    public class JobExecuteOrchestratorTests
    {
        [Test]
        public void Execute_OnInvalidOperationException_ReturnsBadRequest()
        {
            // set up
            int jobId = Faker.RandomNumber.Next();
            string hash = UrlUtils.Encode(jobId);
            string errorMessage = Faker.Lorem.Sentence();
            IExecuteJobCommand executeJobCommand = Substitute.For<IExecuteJobCommand>();
            executeJobCommand.ExecuteAsync(jobId).Throws(new InvalidOperationException(errorMessage));

            // execute
            JobExecuteOrchestrator orchestrator = new JobExecuteOrchestrator(executeJobCommand);
            var result = orchestrator.Execute(hash).Result as BadRequestObjectResult;

            // assert
            executeJobCommand.Received(1).ExecuteAsync(jobId);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.EqualTo(errorMessage));

        }

        [Test]
        public void Execute_OnSuccess_ReturnsOk()
        {
            // set up
            int jobId = Faker.RandomNumber.Next();
            string hash = UrlUtils.Encode(jobId);
            string errorMessage = Faker.Lorem.Sentence();
            IExecuteJobCommand executeJobCommand = Substitute.For<IExecuteJobCommand>();

            // execute
            JobExecuteOrchestrator orchestrator = new JobExecuteOrchestrator(executeJobCommand);
            var result = orchestrator.Execute(hash).Result as OkResult;

            // assert
            executeJobCommand.Received(1).ExecuteAsync(jobId);
            Assert.That(result, Is.Not.Null);
        }

    }
}
