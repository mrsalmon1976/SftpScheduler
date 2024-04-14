using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Quartz;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.ViewOrchestrators.Api.Job;
using SftpSchedulerService.ViewOrchestrators.Api.Report;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Report
{
    [TestFixture]
    public class ReportJobsWithNoTransfersOrchestratorTests
    {
        [Test]
        public void Execute_OnFetch_ReturnsOk()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = AutoMapperTestHelper.CreateMapper();
            JobRepository jobRepo = Substitute.For<JobRepository>();

            JobViewModel[] jobViewModels = { new SubstituteBuilder<JobViewModel>().WithRandomProperties().Build()        };
            JobEntity[] jobEntities = { new SubstituteBuilder<JobEntity>().WithRandomProperties().Build() };

            DateTime startDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;
            jobRepo.GetAllWithoutTransfersBetweenAsync(Arg.Any<IDbContext>(), startDate.ToUniversalTime(), endDate.ToUniversalTime()).Returns(jobEntities);

            IReportJobsWithNoTransfersOrchestrator orchestrator = new ReportJobsWithNoTransfersOrchestrator(dbContextFactory, mapper, jobRepo);
            var result = orchestrator.Execute(startDate, endDate).Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.Not.Null);

            IEnumerable<JobViewModel> jobViewModelResult = (IEnumerable<JobViewModel>)result.Value;

            Assert.IsNotNull(jobViewModelResult);
            Assert.That(jobViewModelResult.Count, Is.EqualTo(1));
        }


    }
}
