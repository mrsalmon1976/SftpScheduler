using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Validators;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.ViewOrchestrators.Api.Job;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

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
            ICreateJobCommand createJobCommand = Substitute.For<ICreateJobCommand>();
            JobViewModel jobViewModel = new SubstituteBuilder<JobViewModel>().WithRandomProperties().Build();
            string userName = RandomData.StringWord();

            createJobCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<JobEntity>(), userName).Throws(new DataValidationException("exception", new ValidationResult(new string[] { "error" })));

            JobCreateOrchestrator jobCreateOrchestrator = new JobCreateOrchestrator(dbContextFactory, mapper, createJobCommand);
            var result = jobCreateOrchestrator.Execute(jobViewModel, userName).Result as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<ValidationResult>());

        }

        [Test]
        public void Execute_OnSave_ReturnsOk()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            ICreateJobCommand createJobCommand = Substitute.For<ICreateJobCommand>();
            JobViewModel jobViewModel = new SubstituteBuilder<JobViewModel>().WithRandomProperties().Build();
            JobEntity jobEntity = new SubstituteBuilder<JobEntity>().WithRandomProperties().Build();
            string userName = RandomData.StringWord();

            mapper.Map<JobEntity>(jobViewModel).Returns(jobEntity);
            mapper.Map<JobViewModel>(Arg.Any<JobEntity>()).Returns(jobViewModel);


            createJobCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<JobEntity>(), userName).Returns(jobEntity);

            JobCreateOrchestrator JobCreateOrchestrator = new JobCreateOrchestrator(dbContextFactory, mapper, createJobCommand);
            var result = JobCreateOrchestrator.Execute(jobViewModel, userName).Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<JobViewModel>());
        }

        [Test]
        public void Execute_OnSave_RunsInTransaction()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IDbContext dbContext = Substitute.For<IDbContext>();
            dbContextFactory.GetDbContext().Returns(dbContext);
            string userName = RandomData.StringWord();

            IMapper mapper = Substitute.For<IMapper>();
            ICreateJobCommand createJobCommand = Substitute.For<ICreateJobCommand>();
            JobViewModel jobViewModel = new SubstituteBuilder<JobViewModel>().WithRandomProperties().Build();
            JobEntity jobEntity = new SubstituteBuilder<JobEntity>().WithRandomProperties().Build();

            mapper.Map<JobEntity>(jobViewModel).Returns(jobEntity);
            mapper.Map<JobViewModel>(Arg.Any<JobEntity>()).Returns(jobViewModel);


            createJobCommand.ExecuteAsync(dbContext, Arg.Any<JobEntity>(), userName).Returns(jobEntity);

            JobCreateOrchestrator JobCreateOrchestrator = new JobCreateOrchestrator(dbContextFactory, mapper, createJobCommand);
            JobCreateOrchestrator.Execute(jobViewModel, userName).GetAwaiter().GetResult();

            dbContext.Received(1).BeginTransaction();
            dbContext.Received(1).Commit();
        }


        [Test]
        public void Execute_OnSave_ReturnsResultWithId()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            ICreateJobCommand createJobCommand = Substitute.For<ICreateJobCommand>();
            JobViewModel jobViewModel = new SubstituteBuilder<JobViewModel>().WithRandomProperties().Build();
            JobViewModel jobViewModelExpected = new SubstituteBuilder<JobViewModel>().WithRandomProperties().Build();
            string userName = RandomData.StringWord();

            JobEntity jobEntity = new SubstituteBuilder<JobEntity>().WithRandomProperties().Build();

            mapper.Map<JobEntity>(jobViewModel).Returns(jobEntity);
            mapper.Map<JobViewModel>(Arg.Any<JobEntity>()).Returns(jobViewModelExpected);


            createJobCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<JobEntity>(), userName).Returns(jobEntity);

            JobCreateOrchestrator JobCreateOrchestrator = new JobCreateOrchestrator(dbContextFactory, mapper, createJobCommand);
            var result = (OkObjectResult)JobCreateOrchestrator.Execute(jobViewModel, userName).Result;
            JobViewModel resultModel = (JobViewModel)result.Value;

            Assert.That(resultModel.Id, Is.EqualTo(jobViewModelExpected.Id));
        }

    }
}
