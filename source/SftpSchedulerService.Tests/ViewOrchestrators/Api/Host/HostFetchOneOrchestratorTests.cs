using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Models.Host;
using SftpSchedulerService.Utilities;
using SftpSchedulerService.ViewOrchestrators.Api.Host;

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Host
{
    [TestFixture]
    public class HostFetchOneOrchestratorTests
    {
        [Test]
        public void Execute_FetchesEntityAndReturnsAsViewModel()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IDbContext dbContext = Substitute.For<IDbContext>();
            dbContextFactory.GetDbContext().Returns(dbContext);

            int hostId = Faker.RandomNumber.Next(10, 1000);
            string hashId = UrlUtils.Encode(hostId);

            HostRepository hostRepo = Substitute.For<HostRepository>();
            var hostEntity = new SubstituteBuilder<HostEntity>().WithRandomProperties().WithProperty(x => x.Id, hostId).Build();
            hostRepo.GetByIdAsync(dbContext, hostId).Returns(hostEntity);

            // execute
            HostFetchOneOrchestrator hostFetchOneOrchestrator = new HostFetchOneOrchestrator(dbContextFactory, hostRepo);
            var result = hostFetchOneOrchestrator.Execute(hashId).Result as OkObjectResult;

            // assert
            Assert.That(result, Is.Not.Null);

            var resultModel = result.Value as HostViewModel;
            Assert.That(resultModel, Is.Not.Null);
            Assert.That(resultModel.HashId, Is.EqualTo(hashId));

        }

    }

}
