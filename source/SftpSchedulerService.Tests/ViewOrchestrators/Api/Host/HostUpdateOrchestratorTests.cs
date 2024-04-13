using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Validators;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Models.Host;
using SftpSchedulerService.ViewOrchestrators.Api.Host;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Host
{
    [TestFixture]
    public class HostUpdateOrchestratorTests
    {
        [Test]
        public void Execute_OnDataValidationException_ReturnsBadRequest()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            IUpdateHostCommand updateHostCommand = Substitute.For<IUpdateHostCommand>();
            HostViewModel hostViewModel = new SubstituteBuilder<HostViewModel>().WithRandomProperties().Build();
            string userName = Guid.NewGuid().ToString();

            updateHostCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<HostEntity>(), Arg.Any<string>()).Throws(new DataValidationException("exception", new ValidationResult(new string[] { "error" })));

            HostUpdateOrchestrator orchestrator = new HostUpdateOrchestrator(dbContextFactory, mapper, updateHostCommand);
            var result = orchestrator.Execute(hostViewModel, userName).Result as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<ValidationResult>());

        }

        [Test]
        public void Execute_OnSave_ReturnsOk()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            IUpdateHostCommand updateHostCommand = Substitute.For<IUpdateHostCommand>();
            HostViewModel hostViewModel = new SubstituteBuilder<HostViewModel>().WithRandomProperties().Build();
            var hostEntity = new SubstituteBuilder<HostEntity>().WithRandomProperties().Build();
            string userName = Guid.NewGuid().ToString();

            mapper.Map<HostEntity>(hostViewModel).Returns(hostEntity);
            mapper.Map<HostViewModel>(Arg.Any<HostEntity>()).Returns(hostViewModel);


            updateHostCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<HostEntity>(), Arg.Any<string>()).Returns(hostEntity);

            HostUpdateOrchestrator orchestrator = new HostUpdateOrchestrator(dbContextFactory, mapper, updateHostCommand);
            var result = orchestrator.Execute(hostViewModel, userName).Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<HostViewModel>());
        }

        [Test]
        public void Execute_OnSave_ExecutesCommandk()
        {
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IDbContextFactory dbContextFactory = new SubstituteBuilder<IDbContextFactory>().Build();
            dbContextFactory.GetDbContext().Returns(dbContext);
            IMapper mapper = Substitute.For<IMapper>();
            IUpdateHostCommand updateHostCommand = Substitute.For<IUpdateHostCommand>();
            HostViewModel hostViewModel = new SubstituteBuilder<HostViewModel>().WithRandomProperties().Build();
            var hostEntity = new SubstituteBuilder<HostEntity>().WithRandomProperties().Build();
            string userName = Guid.NewGuid().ToString();

            mapper.Map<HostEntity>(hostViewModel).Returns(hostEntity);
            mapper.Map<HostViewModel>(Arg.Any<HostEntity>()).Returns(hostViewModel);


            updateHostCommand.ExecuteAsync(dbContext, hostEntity, userName).Returns(hostEntity);

            HostUpdateOrchestrator orchestrator = new HostUpdateOrchestrator(dbContextFactory, mapper, updateHostCommand);
            var result = orchestrator.Execute(hostViewModel, userName).Result as OkObjectResult;

            // assert
            updateHostCommand.Received(1).ExecuteAsync(dbContext, hostEntity, userName);
        }

        [Test]
        public void Execute_OnSave_RunsInTransaction()
        {
            // setup
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IDbContextFactory dbContextFactory = new SubstituteBuilder<IDbContextFactory>().Build();
            dbContextFactory.GetDbContext().Returns(dbContext);
            IMapper mapper = Substitute.For<IMapper>();
            HostViewModel hostViewModel = new SubstituteBuilder<HostViewModel>().WithRandomProperties().Build();
            var hostEntity = new SubstituteBuilder<HostEntity>().WithRandomProperties().Build();
            string userName = Guid.NewGuid().ToString();

            mapper.Map<HostEntity>(hostViewModel).Returns(hostEntity);
            mapper.Map<HostViewModel>(Arg.Any<HostEntity>()).Returns(hostViewModel);


            IUpdateHostCommand updateHostCommand = Substitute.For<IUpdateHostCommand>();
            updateHostCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<HostEntity>(), Arg.Any<string>()).Returns(hostEntity);

            // execute
            HostUpdateOrchestrator orchestrator = new HostUpdateOrchestrator(dbContextFactory, mapper, updateHostCommand);
            orchestrator.Execute(hostViewModel, userName).GetAwaiter().GetResult();

            // assert
            dbContext.Received(1).BeginTransaction();
            dbContext.Received(1).Commit();
        }

    }
}
