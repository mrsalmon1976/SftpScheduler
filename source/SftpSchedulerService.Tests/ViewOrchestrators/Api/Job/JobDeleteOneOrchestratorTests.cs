using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.Utilities;
using SftpSchedulerService.ViewOrchestrators.Api.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Job
{
    [TestFixture]
    public class JobDeleteOneOrchestratorTests
    {
        [Test]
        public void Execute_ExecutesCommand()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IDbContext dbContext = Substitute.For<IDbContext>();
            dbContextFactory.GetDbContext().Returns(dbContext);
            IDeleteJobCommand deleteJobCommand = Substitute.For<IDeleteJobCommand>();
            int jobId = Faker.RandomNumber.Next(10, 1000);
            string hashId = UrlUtils.Encode(jobId);

            JobDeleteOneOrchestrator jobDeleteOneOrchestrator = new JobDeleteOneOrchestrator(Substitute.For<ILogger<JobDeleteOneOrchestrator>>(), dbContextFactory, deleteJobCommand);
            var result = jobDeleteOneOrchestrator.Execute(hashId).Result as OkResult;

            Assert.That(result, Is.Not.Null);
            deleteJobCommand.Received(1).ExecuteAsync(dbContext, jobId).GetAwaiter().GetResult();

        }

    }
}
