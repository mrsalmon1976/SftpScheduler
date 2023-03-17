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
using SftpSchedulerService.ViewOrchestrators.Api.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Job
{
    [TestFixture]
    public class JobCreateOrchestratorTests
    {
        [Test]
        public void Execute_OnDataValidationException_ReturnsBadRequest()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            CreateJobCommand createJobCommand = MockCreateJobCommand();
            JobViewModel jobViewModel = ViewModelTestHelper.CreateJobViewModel();

            createJobCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<JobEntity>()).Throws(new DataValidationException("exception", new ValidationResult(new string[] { "error" })));

            JobCreateOrchestrator jobCreateOrchestrator = new JobCreateOrchestrator(dbContextFactory, mapper, createJobCommand);
            var result = jobCreateOrchestrator.Execute(jobViewModel).Result as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<ValidationResult>());

        }

        [Test]
        public void Execute_OnSave_ReturnsOk()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            CreateJobCommand createJobCommand = MockCreateJobCommand();
            JobViewModel jobViewModel = ViewModelTestHelper.CreateJobViewModel();
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();

            mapper.Map<JobEntity>(jobViewModel).Returns(jobEntity);
            mapper.Map<JobViewModel>(Arg.Any<JobEntity>()).Returns(jobViewModel);


            createJobCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<JobEntity>()).Returns(jobEntity);

            JobCreateOrchestrator JobCreateOrchestrator = new JobCreateOrchestrator(dbContextFactory, mapper, createJobCommand);
            var result = JobCreateOrchestrator.Execute(jobViewModel).Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<JobViewModel>());
        }

        [Test]
        public void Execute_OnSave_ReturnsResultWithId()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            CreateJobCommand createJobCommand = MockCreateJobCommand();
            JobViewModel jobViewModel = ViewModelTestHelper.CreateJobViewModel();
            JobViewModel jobViewModelExpected = ViewModelTestHelper.CreateJobViewModel();
            jobViewModelExpected.Id = Faker.RandomNumber.Next();
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();

            mapper.Map<JobEntity>(jobViewModel).Returns(jobEntity);
            mapper.Map<JobViewModel>(Arg.Any<JobEntity>()).Returns(jobViewModelExpected);


            createJobCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<JobEntity>()).Returns(jobEntity);

            JobCreateOrchestrator JobCreateOrchestrator = new JobCreateOrchestrator(dbContextFactory, mapper, createJobCommand);
            var result = (OkObjectResult)JobCreateOrchestrator.Execute(jobViewModel).Result;
            JobViewModel JobViewModelResult = (JobViewModel)result.Value;

            Assert.That(JobViewModelResult.Id, Is.EqualTo(jobViewModelExpected.Id));
        }

        private CreateJobCommand MockCreateJobCommand()
        {
            return Substitute.For<CreateJobCommand>(Substitute.For<IJobValidator>(), Substitute.For<ISchedulerFactory>());
        }

    }
}
