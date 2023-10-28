using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Tests.Builders.Models;
using SftpScheduler.BLL.Validators;
using SftpSchedulerService.Models.Host;
using SftpSchedulerService.Tests.Builders.Models.Host;
using SftpSchedulerService.ViewOrchestrators.Api.Host;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Host
{
    [TestFixture]
    public class HostCreateOrchestratorTests
    {
        [Test]
        public void Execute_OnDataValidationException_ReturnsBadRequest()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            ICreateHostCommand createHostCommand = Substitute.For<ICreateHostCommand>();
            HostViewModel hostViewModel = new HostViewModelBuilder().WithRandomProperties().Build();

            createHostCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<HostEntity>()).Throws(new DataValidationException("exception", new ValidationResult(new string[] { "error" })));

            HostCreateOrchestrator hostCreateOrchestrator = new HostCreateOrchestrator(dbContextFactory, mapper, createHostCommand);
            var result = hostCreateOrchestrator.Execute(hostViewModel).Result as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<ValidationResult>());

        }

        [Test]
        public void Execute_OnSave_ReturnsOk()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            ICreateHostCommand createHostCommand = Substitute.For<ICreateHostCommand>();
            HostViewModel hostViewModel = new HostViewModelBuilder().WithRandomProperties().Build();
            HostEntity hostEntity = new HostEntityBuilder().WithRandomProperties().Build();

            mapper.Map<HostEntity>(hostViewModel).Returns(hostEntity);
            mapper.Map<HostViewModel>(Arg.Any<HostEntity>()).Returns(hostViewModel);


            createHostCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<HostEntity>()).Returns(hostEntity);

            HostCreateOrchestrator hostCreateOrchestrator = new HostCreateOrchestrator(dbContextFactory, mapper, createHostCommand);
            var result = hostCreateOrchestrator.Execute(hostViewModel).Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<HostViewModel>());
        }

        [Test]
        public void Execute_OnSave_ReturnsResultWithId()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            ICreateHostCommand createHostCommand = Substitute.For<ICreateHostCommand>();
            HostViewModel hostViewModel = new HostViewModelBuilder().WithRandomProperties().Build();
            HostViewModel hostViewModelExpected = new HostViewModelBuilder().WithRandomProperties().Build();
            HostEntity hostEntity = new HostEntityBuilder().WithRandomProperties().Build();

            mapper.Map<HostEntity>(hostViewModel).Returns(hostEntity);
            mapper.Map<HostViewModel>(Arg.Any<HostEntity>()).Returns(hostViewModelExpected);


            createHostCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<HostEntity>()).Returns(hostEntity);

            HostCreateOrchestrator hostCreateOrchestrator = new HostCreateOrchestrator(dbContextFactory, mapper, createHostCommand);
            var result = (OkObjectResult)hostCreateOrchestrator.Execute(hostViewModel).Result;
            HostViewModel hostViewModelResult = (HostViewModel)result.Value;

            Assert.That(hostViewModelResult.Id, Is.EqualTo(hostViewModelExpected.Id));
        }

    }
}
