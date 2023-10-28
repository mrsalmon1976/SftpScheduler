using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Tests.Builders.Models;
using SftpScheduler.BLL.Validators;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.ViewOrchestrators.Api.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Notification
{
    [TestFixture]
    public class JobUpdateOrchestratorTests
    {
        [Test]
        public void Execute_OnDataValidationException_ReturnsBadRequest()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            IUpdateJobCommand updateJobCommand = Substitute.For<IUpdateJobCommand>();
            JobViewModel jobViewModel = ViewModelTestHelper.CreateJobViewModel();

            updateJobCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<JobEntity>()).Throws(new DataValidationException("exception", new ValidationResult(new string[] { "error" })));

            JobUpdateOrchestrator jobUpdateOrchestrator = new JobUpdateOrchestrator(dbContextFactory, mapper, updateJobCommand);
            var result = jobUpdateOrchestrator.Execute(jobViewModel).Result as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<ValidationResult>());

        }

        [Test]
        public void Execute_OnSave_ReturnsOk()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            IUpdateJobCommand updateJobCommand = Substitute.For<IUpdateJobCommand>();
            JobViewModel jobViewModel = ViewModelTestHelper.CreateJobViewModel();
            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().Build();

            mapper.Map<JobEntity>(jobViewModel).Returns(jobEntity);
            mapper.Map<JobViewModel>(Arg.Any<JobEntity>()).Returns(jobViewModel);


            updateJobCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<JobEntity>()).Returns(jobEntity);

            JobUpdateOrchestrator JobUpdateOrchestrator = new JobUpdateOrchestrator(dbContextFactory, mapper, updateJobCommand);
            var result = JobUpdateOrchestrator.Execute(jobViewModel).Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<JobViewModel>());
        }

        [Test]
        public void Execute_OnSave_ReturnsResultWithId()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            IUpdateJobCommand updateJobCommand = Substitute.For<IUpdateJobCommand>();
            JobViewModel jobViewModel = ViewModelTestHelper.CreateJobViewModel();
            JobViewModel jobViewModelExpected = ViewModelTestHelper.CreateJobViewModel();
            jobViewModelExpected.Id = Faker.RandomNumber.Next();
            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().Build();

            mapper.Map<JobEntity>(jobViewModel).Returns(jobEntity);
            mapper.Map<JobViewModel>(Arg.Any<JobEntity>()).Returns(jobViewModelExpected);


            updateJobCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<JobEntity>()).Returns(jobEntity);

            JobUpdateOrchestrator JobUpdateOrchestrator = new JobUpdateOrchestrator(dbContextFactory, mapper, updateJobCommand);
            var result = (OkObjectResult)JobUpdateOrchestrator.Execute(jobViewModel).Result;
            JobViewModel resultModel = (JobViewModel)result.Value;

            Assert.That(resultModel.Id, Is.EqualTo(jobViewModelExpected.Id));
        }

    }
}
