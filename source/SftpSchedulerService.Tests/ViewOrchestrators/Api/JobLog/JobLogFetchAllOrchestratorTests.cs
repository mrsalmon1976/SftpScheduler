using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Tests.Builders.Models;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.Utilities;
using SftpSchedulerService.ViewOrchestrators.Api.JobLog;
using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.JobLog
{
    [TestFixture]
    public class JobLogFetchAllOrchestratorTests
    {
        [Test]
        public void Execute_OnFetch_ReturnsOk()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = AutoMapperTestHelper.CreateMapper();
            JobLogRepository jobLogRepo = Substitute.For<JobLogRepository>();
            int jobId = Faker.RandomNumber.Next(1, 100);
            string jobHash = UrlUtils.Encode(jobId);
            int maxLogId = Faker.RandomNumber.Next(1, 10000);

            JobLogViewModel[] jobLogViewModels = { ViewModelTestHelper.CreateJobLogViewModel() };
            JobLogEntity[] jobLogEntities = { new JobLogEntityBuilder().WithRandomProperties().WithJobId(jobId).Build() };

            jobLogRepo.GetAllByJobAsync(Arg.Any<IDbContext>(), jobId, maxLogId, JobLogFetchAllOrchestrator.DefaultRowCount).Returns(jobLogEntities);


            JobLogFetchAllOrchestrator jobLogFetchAllOrchestrator = new JobLogFetchAllOrchestrator(dbContextFactory, mapper, jobLogRepo);
            var result = jobLogFetchAllOrchestrator.Execute(jobHash, maxLogId).Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.Not.Null);

            IEnumerable<JobLogViewModel> jobViewModelResult = (IEnumerable<JobLogViewModel>)result.Value;

            Assert.IsNotNull(jobViewModelResult);
            Assert.That(jobViewModelResult.Count, Is.EqualTo(1));
            jobLogRepo.Received(1).GetAllByJobAsync(Arg.Any<IDbContext>(), jobId, maxLogId, JobLogFetchAllOrchestrator.DefaultRowCount);
        }


    }
}
