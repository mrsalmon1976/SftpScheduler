using NSubstitute;
using NUnit.Framework;
using Quartz;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace SftpScheduler.BLL.Tests.Commands.Host
{
    [TestFixture]
    public class DeleteHostCommandTests
    {

        [Test]
        public void Execute_JobsStillAttached_ThrowsException()
        {
            HostRepository hostRepo = Substitute.For<HostRepository>(); 
            IDbContext dbContext = Substitute.For<IDbContext>();
            int hostId = Faker.RandomNumber.Next(5, 1000);

            hostRepo.GetJobCountAsync(dbContext, hostId).Returns(1);

            // execute
            DeleteHostCommand deleteHostCommand = new DeleteHostCommand(hostRepo);
            bool exceptionThrown = false;
            try
            {
                deleteHostCommand.ExecuteAsync(dbContext, hostId).GetAwaiter().GetResult();
            }
            catch (InvalidOperationException ex)
            {
                Assert.That(ex.Message.Contains("Host cannot be deleted"));
            }

            // assert
            Assert.That(exceptionThrown, Is.False);
            hostRepo.Received(1).GetJobCountAsync(dbContext, hostId).GetAwaiter().GetResult();
            dbContext.Received(0).ExecuteNonQueryAsync(Arg.Any<string>(), Arg.Any<object>());

        }

        [TestCase("Host")]
        public void Execute_OnDelete_RunsDeleteOnTable(string tableName)
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            int hostId = Faker.RandomNumber.Next(5, 1000);
            bool deleteFound = false;

            dbContext.When(x => x.ExecuteNonQueryAsync(Arg.Any<string>(), Arg.Any<object>())).Do((ci) =>
            {
                string executedSql = ci.ArgAt<string>(0);
                if (executedSql.Contains(tableName))
                {
                    deleteFound = true;
                }
            });

            DeleteHostCommand deleteHostCommand = new DeleteHostCommand(Substitute.For<HostRepository>());
            deleteHostCommand.ExecuteAsync(dbContext, hostId).GetAwaiter().GetResult();

            Assert.That(deleteFound, $"No delete on table {tableName} occurred");
        }

        [Test]
        public void ExecuteAsync_Integration_DeletesFromAllTables()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();
                HostRepository hostRepo = new HostRepository();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    DeleteHostCommand deleteHostCommand = new DeleteHostCommand(hostRepo);
                    HostEntity host = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    int hostId = host.Id;

                    deleteHostCommand.ExecuteAsync(dbContext, hostId).GetAwaiter().GetResult();

                    HostEntity hostAfterDelete = hostRepo.GetByIdOrDefaultAsync(dbContext, hostId).GetAwaiter().GetResult();
                    Assert.IsNull(hostAfterDelete);


                }
            }
        }


    }
}
