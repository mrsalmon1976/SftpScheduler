using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.ViewOrchestrators.Api.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Job
{
    [TestFixture]
    public class JobFetchAllOrchestratorTests
    {
        [Test]
        public void Execute_OnSave_ReturnsOk()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            JobRepository jobRepo = Substitute.For<JobRepository>();

            JobViewModel[] jobViewModels = { ViewModelTestHelper.CreateJobViewModel() };
            JobEntity[] jobEntities = { EntityTestHelper.CreateJobEntity() };

            jobRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(jobEntities);
            mapper.Map<JobEntity[], JobViewModel[]>(Arg.Any<JobEntity[]>()).Returns(jobViewModels);


            JobFetchAllOrchestrator jobFetchAllProvider = new JobFetchAllOrchestrator(dbContextFactory, mapper, jobRepo);
            var result = jobFetchAllProvider.Execute().Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.Not.Null);

            IEnumerable<JobViewModel> jobViewModelResult = (IEnumerable<JobViewModel>)result.Value;

            Assert.IsNotNull(jobViewModelResult);
            Assert.That(jobViewModelResult.Count, Is.EqualTo(1));
        }

    }
}
