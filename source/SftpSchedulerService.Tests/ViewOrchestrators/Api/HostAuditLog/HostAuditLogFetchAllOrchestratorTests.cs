using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Models.HostAuditLog;
using SftpSchedulerService.Utilities;
using SftpSchedulerService.ViewOrchestrators.Api.HostAuditLog;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.HostAuditLog
{
    [TestFixture]
    public class HostAuditLogFetchAllOrchestratorTests
    {
        [Test]
        public void Execute_OnFetch_ReturnsOk()
        {
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IDbContextFactory dbContextFactory = new SubstituteBuilder<IDbContextFactory>().Build();
            dbContextFactory.GetDbContext().Returns(dbContext);
            IHostAuditLogRepository hostAuditLogRepo = Substitute.For<IHostAuditLogRepository>();
            int hostId = Faker.RandomNumber.Next(1, 100);
            string hostHash = UrlUtils.Encode(hostId);

            HostAuditLogViewModel[] viewModels = { new SubstituteBuilder<HostAuditLogViewModel>().WithRandomProperties().WithProperty(x => x.HostId, hostId).Build() };
            HostAuditLogEntity[] hostAuditLogEntities = { new SubstituteBuilder<HostAuditLogEntity>().WithRandomProperties().WithProperty(x => x.HostId, hostId).Build() };

            hostAuditLogRepo.GetByAllHostAsync(dbContext, hostId).Returns(hostAuditLogEntities);

            // execute
            HostAuditLogFetchAllOrchestrator hostAuditLogFetchAllOrchestrator = new HostAuditLogFetchAllOrchestrator(dbContextFactory, hostAuditLogRepo);
            var result = hostAuditLogFetchAllOrchestrator.Execute(hostHash).Result as OkObjectResult;

            // assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.Not.Null);

            IEnumerable<HostAuditLogViewModel> auditLogViewModelResult = (IEnumerable<HostAuditLogViewModel>)result.Value;

            Assert.IsNotNull(auditLogViewModelResult);
            Assert.That(auditLogViewModelResult.Count, Is.EqualTo(1));
            hostAuditLogRepo.Received(1).GetByAllHostAsync(dbContext, hostId);
        }


    }
}
