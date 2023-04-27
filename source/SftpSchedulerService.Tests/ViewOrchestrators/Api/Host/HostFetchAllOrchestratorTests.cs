using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
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
            IMapper mapper = CreateMapper();
            HostRepository hostRepo = Substitute.For<HostRepository>();

            HostViewModel[] hostViewModels = { ViewModelTestHelper.CreateHostViewModel() };
            HostEntity[] hostEntities = { EntityTestHelper.CreateHostEntity() };

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
            IMapper mapper = CreateMapper();

            HostEntity hostEntity1 = EntityTestHelper.CreateHostEntity(111);
            HostEntity hostEntity2 = EntityTestHelper.CreateHostEntity(222);
            HostEntity[] hostEntities = { hostEntity1, hostEntity2 };
            hostRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(hostEntities);

            // set up some job counts
            HostJobCountEntity hostJobCountEntity1 = EntityTestHelper.CreateHostJobCountEntity(hostEntity1.Id);
            HostJobCountEntity hostJobCountEntity2 = EntityTestHelper.CreateHostJobCountEntity(hostEntity2.Id);
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

        private static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<HostEntity, HostViewModel>());
            return config.CreateMapper();

        }

    }
}
