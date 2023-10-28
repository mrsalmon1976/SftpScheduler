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
using SftpSchedulerService.ViewOrchestrators.Api.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            HostViewModel hostViewModel = ViewModelTestHelper.CreateHostViewModel();

            updateHostCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<HostEntity>()).Throws(new DataValidationException("exception", new ValidationResult(new string[] { "error" })));

            HostUpdateOrchestrator orchestrator = new HostUpdateOrchestrator(dbContextFactory, mapper, updateHostCommand);
            var result = orchestrator.Execute(hostViewModel).Result as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<ValidationResult>());

        }

        [Test]
        public void Execute_OnSave_ReturnsOk()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            IUpdateHostCommand updateHostCommand = Substitute.For<IUpdateHostCommand>();
            HostViewModel hostViewModel = ViewModelTestHelper.CreateHostViewModel();
            HostEntity hostEntity = new HostEntityBuilder().WithRandomProperties().Build();

            mapper.Map<HostEntity>(hostViewModel).Returns(hostEntity);
            mapper.Map<HostViewModel>(Arg.Any<HostEntity>()).Returns(hostViewModel);


            updateHostCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<HostEntity>()).Returns(hostEntity);

            HostUpdateOrchestrator orchestrator = new HostUpdateOrchestrator(dbContextFactory, mapper, updateHostCommand);
            var result = orchestrator.Execute(hostViewModel).Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<HostViewModel>());
        }

    }
}
