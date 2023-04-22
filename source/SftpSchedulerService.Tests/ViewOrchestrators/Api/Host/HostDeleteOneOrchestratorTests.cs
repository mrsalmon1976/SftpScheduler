using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpSchedulerService.Utilities;
using SftpSchedulerService.ViewOrchestrators.Api.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Job
{
    [TestFixture]
    public class HostDeleteOneOrchestratorTests
    {
        [Test]
        public void Execute_ExecutesCommand()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IDbContext dbContext = Substitute.For<IDbContext>();
            dbContextFactory.GetDbContext().Returns(dbContext);
            IDeleteHostCommand deleteHostCmd = Substitute.For<IDeleteHostCommand>();
            int hostId = Faker.RandomNumber.Next(10, 1000);
            string hashId = UrlUtils.Encode(hostId);

            HostDeleteOneOrchestrator hostDeleteOneOrchestrator = new HostDeleteOneOrchestrator(Substitute.For<ILogger<HostDeleteOneOrchestrator>>(), dbContextFactory, deleteHostCmd);
            var result = hostDeleteOneOrchestrator.Execute(hashId).Result as OkResult;

            Assert.That(result, Is.Not.Null);
            deleteHostCmd.Received(1).ExecuteAsync(dbContext, hostId).GetAwaiter().GetResult();

        }

    }
}
