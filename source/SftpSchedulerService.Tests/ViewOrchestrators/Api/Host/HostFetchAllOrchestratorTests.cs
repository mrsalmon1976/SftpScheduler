using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Tests.Builders.Models;
using SftpScheduler.BLL.Validators;
using SftpSchedulerService.Models.Host;
using SftpSchedulerService.ViewOrchestrators.Api.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8604 // Possible null reference argument.

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Host
{
    [TestFixture]
    public class HostFetchAllOrchestratorTests
    {
        [Test]
        public void Execute_ReturnsOk()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = AutoMapperTestHelper.CreateMapper();
            HostRepository hostRepo = Substitute.For<HostRepository>();

            HostViewModel[] hostViewModels = { ViewModelTestHelper.CreateHostViewModel() };
            HostEntity[] hostEntities = { new HostEntityBuilder().Build() };

            hostRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(hostEntities);


            HostFetchAllOrchestrator hostFetchAllProvider = new HostFetchAllOrchestrator(dbContextFactory, mapper, hostRepo);
            var result = hostFetchAllProvider.Execute().Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.Not.Null);

            IEnumerable<HostViewModel> hostViewModelResult = (IEnumerable<HostViewModel>)result.Value;

            Assert.IsNotNull(hostViewModelResult);
            Assert.That(hostViewModelResult.Count, Is.EqualTo(1));
        }

        [Test]
        public void Execute_HydratesJobCounts()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            HostRepository hostRepo = Substitute.For<HostRepository>();
            IMapper mapper = AutoMapperTestHelper.CreateMapper();

            HostEntity hostEntity1 = new HostEntityBuilder().WithId(111).Build();
            HostEntity hostEntity2 = new HostEntityBuilder().WithId(222).Build();
            HostEntity[] hostEntities = { hostEntity1, hostEntity2 };
            hostRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(hostEntities);

            // set up some job counts
            HostJobCountEntity hostJobCountEntity1 = new HostJobCountEntityBuilder().WithHostId(hostEntity1.Id).Build();
            HostJobCountEntity hostJobCountEntity2 = new HostJobCountEntityBuilder().WithHostId(hostEntity2.Id).Build();
            HostJobCountEntity[] hostJobCountEntities = { hostJobCountEntity1, hostJobCountEntity2 };
            hostRepo.GetAllJobCountsAsync(Arg.Any<IDbContext>()).Returns(hostJobCountEntities);


            HostFetchAllOrchestrator hostFetchAllProvider = new HostFetchAllOrchestrator(dbContextFactory, mapper, hostRepo);
            var result = hostFetchAllProvider.Execute().Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.Not.Null);

            IEnumerable<HostViewModel> hostViewModelResult = (IEnumerable<HostViewModel>)result.Value;

            Assert.That(hostViewModelResult.Single(x => x.Id == hostEntity1.Id).JobCount, Is.EqualTo(hostJobCountEntity1.JobCount));
            Assert.That(hostViewModelResult.Single(x => x.Id == hostEntity2.Id).JobCount, Is.EqualTo(hostJobCountEntity2.JobCount));
        }

    }
}
