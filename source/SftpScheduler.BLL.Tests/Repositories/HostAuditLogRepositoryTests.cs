using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Tests.Builders.Models;
using SftpScheduler.Test.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Tests.Repositories
{
    [TestFixture]
    public class HostAuditLogRepositoryTests
    {



        #region GetByAllHostAsync Tests

        [Test]
        public void GetByAllHostAsync_OnExecute_PerformsQuery()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            var hostEntity = new SubstituteBuilder<HostEntity>().WithRandomProperties().Build();
            dbContext.QueryAsync<HostEntity>(Arg.Any<string>(), Arg.Any<object>()).Returns(new[] { hostEntity });
            
            IHostAuditLogRepository hostAuditLogRepository = new HostAuditLogRepository();
            hostAuditLogRepository.GetByAllHostAsync(dbContext, hostEntity.Id).GetAwaiter().GetResult();

            dbContext.Received(1).QueryAsync<HostAuditLogEntity>(Arg.Any<string>(), Arg.Any<object>());

        }

        [Test]
        public void GetByAllHostAsync_Integration_ReturnsDbRecord()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);
                    HostAuditLogEntity hostAuditLogEntity1 = dbIntegrationTestHelper.CreateHostAuditLogEntity(dbContext, hostEntity.Id);
                    HostAuditLogEntity hostAuditLogEntity2 = dbIntegrationTestHelper.CreateHostAuditLogEntity(dbContext, hostEntity.Id);

                    HostAuditLogRepository hostAuditLogRepo = new HostAuditLogRepository();
                    List<HostAuditLogEntity> result = hostAuditLogRepo.GetByAllHostAsync(dbContext, hostEntity.Id).GetAwaiter().GetResult().ToList();

                    Assert.That(result.Count, Is.EqualTo(2)); ;
                    Assert.That(result[0].PropertyName, Is.EqualTo(hostAuditLogEntity1.PropertyName));
                    Assert.That(result[1].PropertyName, Is.EqualTo(hostAuditLogEntity2.PropertyName));
                }
            }
        }

        #endregion

    }
}
