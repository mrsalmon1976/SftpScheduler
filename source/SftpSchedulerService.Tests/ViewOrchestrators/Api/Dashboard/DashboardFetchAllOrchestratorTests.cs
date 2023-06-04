using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Validators;
using SftpSchedulerService.Models.Dashboard;
using SftpSchedulerService.Models.Host;
using SftpSchedulerService.ViewOrchestrators.Api.Dashboard;
using SftpSchedulerService.ViewOrchestrators.Api.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8604 // Possible null reference argument.

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Dashboard
{
    [TestFixture]
    public class DashboardFetchAllOrchestratorTests
    {

        [Test]
        public void Execute_LoadsStatus_ReturnsPopulatedModel()
        {
            // setup
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IDbContext dbContext = Substitute.For<IDbContext>();
            dbContextFactory.GetDbContext().Returns(dbContext);
            JobRepository jobRepo = Substitute.For<JobRepository>();

            int totalCount = Faker.RandomNumber.Next(10, 30);
            int failedCount = Faker.RandomNumber.Next(1, 10);
            IEnumerable<JobEntity> failedJobs = CreateJobEntities(failedCount);

            jobRepo.GetAllCountAsync(dbContext).Returns(totalCount);
            jobRepo.GetAllFailingAsync(dbContext).Returns(failedJobs);

            // execute
            IDashboardFetchAllOrchestrator orchestrator = CreateOrchestrator(dbContextFactory, jobRepo);
            var result = orchestrator.Execute().Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);

            DashboardViewModel viewModelResult = (result.Value as DashboardViewModel)!;

            Assert.That(viewModelResult, Is.Not.Null);
            Assert.That(viewModelResult.JobFailingCount, Is.EqualTo(failedCount));
            Assert.That(viewModelResult.JobTotalCount, Is.EqualTo(totalCount));
        }

        private IEnumerable<JobEntity> CreateJobEntities(int count)
        {
            List<JobEntity> jobs = new List<JobEntity>();
            for (int i=0; i< count; i++)
            {
                int hostId = Faker.RandomNumber.Next();
                JobEntity jobEntity = EntityTestHelper.CreateJobEntity(hostId);
                jobs.Add(jobEntity);
            }
            return jobs;
        }

        private IDashboardFetchAllOrchestrator CreateOrchestrator(IDbContextFactory dbContextFactory, JobRepository jobRepo)
        {
            return new DashboardFetchAllOrchestrator(dbContextFactory, jobRepo);
        }

    }
}
