using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Validators;
using SftpSchedulerService.Models;
using SftpSchedulerService.ViewProviders.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Tests.ViewProviders.Host
{
    [TestFixture]
    public class PostHostCreateProviderTests
    {
        [Test]
        public void Execute_OnDataValidationException_ReturnsBadRequest()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            ICreateHostCommand createHostCommand = Substitute.For<ICreateHostCommand>();
            HostViewModel hostViewModel = ViewModelTestHelper.CreateHostViewModel();

            createHostCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<HostEntity>()).Throws(new DataValidationException("exception", new ValidationResult(new string[] { "error" })));

            PostHostCreateProvider postHostCreateProvider = new PostHostCreateProvider(dbContextFactory, mapper, createHostCommand);
            var result = postHostCreateProvider.Execute(hostViewModel).Result as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<ValidationResult>());

        }

        [Test]
        public void Execute_OnSave_ReturnsOk()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            ICreateHostCommand createHostCommand = Substitute.For<ICreateHostCommand>();
            HostViewModel hostViewModel = ViewModelTestHelper.CreateHostViewModel();
            HostEntity hostEntity = EntityTestHelper.CreateHostEntity();

            mapper.Map<HostEntity>(hostViewModel).Returns(hostEntity);
            mapper.Map<HostViewModel>(Arg.Any<HostEntity>()).Returns(hostViewModel);


            createHostCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<HostEntity>()).Returns(hostEntity);

            PostHostCreateProvider postHostCreateProvider = new PostHostCreateProvider(dbContextFactory, mapper, createHostCommand);
            var result = postHostCreateProvider.Execute(hostViewModel).Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<HostViewModel>());
        }

        [Test]
        public void Execute_OnSave_ReturnsResultWithId()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            ICreateHostCommand createHostCommand = Substitute.For<ICreateHostCommand>();
            HostViewModel hostViewModel = ViewModelTestHelper.CreateHostViewModel();
            HostViewModel hostViewModelExpected = ViewModelTestHelper.CreateHostViewModel();
            hostViewModelExpected.Id = Faker.RandomNumber.Next();
            HostEntity hostEntity = EntityTestHelper.CreateHostEntity();

            mapper.Map<HostEntity>(hostViewModel).Returns(hostEntity);
            mapper.Map<HostViewModel>(Arg.Any<HostEntity>()).Returns(hostViewModelExpected);


            createHostCommand.ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<HostEntity>()).Returns(hostEntity);

            PostHostCreateProvider postHostCreateProvider = new PostHostCreateProvider(dbContextFactory, mapper, createHostCommand);
            var result = (OkObjectResult)postHostCreateProvider.Execute(hostViewModel).Result;
            HostViewModel hostViewModelResult = (HostViewModel)result.Value;

            Assert.That(hostViewModelResult.Id, Is.EqualTo(hostViewModelExpected.Id));
        }

    }
}
