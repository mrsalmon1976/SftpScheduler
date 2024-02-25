using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Models.Host;
using SftpSchedulerService.ViewOrchestrators.Api.Host;

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

            HostViewModel[] hostViewModels = { new SubstituteBuilder<HostViewModel>().WithRandomProperties().Build() };
            HostEntity[] hostEntities = { new SubstituteBuilder<HostEntity>().Build() };

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

            var hostEntity1 = new SubstituteBuilder<HostEntity>().WithProperty(x => x.Id, 111).Build();
            var hostEntity2 = new SubstituteBuilder<HostEntity>().WithProperty(x => x.Id, 222).Build();
            HostEntity[] hostEntities = { hostEntity1, hostEntity2 };
            hostRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(hostEntities);

            // set up some job counts
            HostJobCountEntity hostJobCountEntity1 = new SubstituteBuilder<HostJobCountEntity>().WithProperty(x => x.HostId, hostEntity1.Id).Build();
            HostJobCountEntity hostJobCountEntity2 = new SubstituteBuilder<HostJobCountEntity>().WithProperty(x => x.HostId, hostEntity2.Id).Build();
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
