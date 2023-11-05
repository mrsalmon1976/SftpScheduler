using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Tests.Builders.Models;

namespace SftpScheduler.BLL.Tests.Repositories
{
    [TestFixture]
    public class JobAuditLogRepositoryTests
    {



        #region GetByAllJobAsync Tests

        [Test]
        public void GetByAllJobAsync_OnExecute_PerformsQuery()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().Build();
            dbContext.QueryAsync<JobEntity>(Arg.Any<string>(), Arg.Any<object>()).Returns(new[] { jobEntity });
            
            IJobAuditLogRepository hostAuditLogRepository = new JobAuditLogRepository();
            hostAuditLogRepository.GetByAllJobAsync(dbContext, jobEntity.Id).GetAwaiter().GetResult();

            dbContext.Received(1).QueryAsync<JobAuditLogEntity>(Arg.Any<string>(), Arg.Any<object>());

        }

        [Test]
        public void GetByAllJobAsync_Integration_ReturnsDbRecord()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {

                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);
                    JobEntity jobEntity = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    JobAuditLogEntity jobAuditLogEntity1 = dbIntegrationTestHelper.CreateJobAuditLogEntity(dbContext, jobEntity.Id);
                    JobAuditLogEntity jobAuditLogEntity2 = dbIntegrationTestHelper.CreateJobAuditLogEntity(dbContext, jobEntity.Id);

                    JobAuditLogRepository jobAuditLogRepo = new JobAuditLogRepository();
                    List<JobAuditLogEntity> result = jobAuditLogRepo.GetByAllJobAsync(dbContext, jobEntity.Id).GetAwaiter().GetResult().ToList();

                    Assert.That(result.Count, Is.EqualTo(2)); ;
                    Assert.That(result[0].PropertyName, Is.EqualTo(jobAuditLogEntity1.PropertyName));
                    Assert.That(result[1].PropertyName, Is.EqualTo(jobAuditLogEntity2.PropertyName));
                }
            }
        }

        #endregion

    }
}
