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

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Notification
{
    [TestFixture]
    public class JobUpdateOrchestratorTests
    {
        [Test]
        public void Execute_OnDataValidationException_ReturnsBadRequest()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IUpdateJobCommand updateJobCommand = Substitute.For<IUpdateJobCommand>();
            JobViewModel jobViewModel = new SubstituteBuilder<JobViewModel>().WithRandomProperties().Build();
            string userName = Guid.NewGuid().ToString();

            updateJobCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<JobEntity>(), userName).Throws(new DataValidationException("exception", new ValidationResult(new string[] { "error" })));

            JobUpdateOrchestrator jobUpdateOrchestrator = new JobUpdateOrchestrator(dbContextFactory, updateJobCommand);
            var result = jobUpdateOrchestrator.Execute(jobViewModel, userName).Result as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<ValidationResult>());

        }

        [Test]
        public void Execute_OnSave_ReturnsOk()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IUpdateJobCommand updateJobCommand = Substitute.For<IUpdateJobCommand>();
            JobViewModel jobViewModel = new SubstituteBuilder<JobViewModel>().WithRandomProperties().Build();
            JobEntity jobEntity = new SubstituteBuilder<JobEntity>().WithRandomProperties().Build();
            string userName = Guid.NewGuid().ToString();

            updateJobCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<JobEntity>(), userName).Returns(jobEntity);

            JobUpdateOrchestrator jobUpdateOrchestrator = new JobUpdateOrchestrator(dbContextFactory, updateJobCommand);
            var result = jobUpdateOrchestrator.Execute(jobViewModel, userName).Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<JobViewModel>());
        }

        [Test]
        public void Execute_OnSave_ExecutesUpdateInTransaction()
        {
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IDbContextFactory dbContextFactory = new SubstituteBuilder<IDbContextFactory>().Build();
            dbContextFactory.GetDbContext().Returns(dbContext);

            IUpdateJobCommand updateJobCommand = Substitute.For<IUpdateJobCommand>();
            JobViewModel jobViewModel = new SubstituteBuilder<JobViewModel>().WithRandomProperties().Build();
            JobEntity jobEntity = new SubstituteBuilder<JobEntity>().WithRandomProperties().Build();
            string userName = Guid.NewGuid().ToString();

            updateJobCommand.ExecuteAsync(dbContext, Arg.Any<JobEntity>(), userName).Returns(jobEntity);

            // execute
            JobUpdateOrchestrator jobUpdateOrchestrator = new JobUpdateOrchestrator(dbContextFactory, updateJobCommand);
            jobUpdateOrchestrator.Execute(jobViewModel, userName).GetAwaiter().GetResult();

            // assert
            dbContext.Received(1).BeginTransaction();
            dbContext.Received(1).Commit();
            updateJobCommand.Received(1).ExecuteAsync(dbContext, Arg.Any<JobEntity>(), userName);
        }


        [Test]
        public void Execute_OnSave_ReturnsResultWithId()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IUpdateJobCommand updateJobCommand = Substitute.For<IUpdateJobCommand>();
            JobViewModel jobViewModel = new SubstituteBuilder<JobViewModel>().WithRandomProperties().Build();
            JobEntity jobEntity = new SubstituteBuilder<JobEntity>().WithRandomProperties().Build();
            string userName = Guid.NewGuid().ToString();

            updateJobCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<JobEntity>(), userName).Returns(jobEntity);

            JobUpdateOrchestrator jobUpdateOrchestrator = new JobUpdateOrchestrator(dbContextFactory, updateJobCommand);
            var result = (OkObjectResult)jobUpdateOrchestrator.Execute(jobViewModel, userName).Result;
            JobViewModel resultModel = (JobViewModel)result.Value;

            Assert.That(resultModel.Id, Is.EqualTo(jobEntity.Id));
        }

    }
}
