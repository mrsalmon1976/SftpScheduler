using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Tests.Builders.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace SftpScheduler.BLL.Tests.Commands.Job
{
    [TestFixture]
    public class CreateJobFileLogCommandTests
    {


        [Test]
        public void ExecuteAsync_Integration_ExecutesQueryWithoutError()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    CreateJobFileLogCommand createJobFileLogCmd = new CreateJobFileLogCommand();
                    HostEntity host = dbIntegrationTestHelper.CreateHostEntity(dbContext);
                    JobEntity job = dbIntegrationTestHelper.CreateJobEntity(dbContext, host.Id);

                    JobFileLogEntity jobFileLog = new JobFileLogEntityBuilder().WithRandomProperties().WithJobId(job.Id).Build();
                    
                    JobFileLogEntity result = createJobFileLogCmd.ExecuteAsync(dbContext, jobFileLog).GetAwaiter().GetResult();

                    Assert.IsNotNull(result);
                    Assert.That(result.Id, Is.GreaterThan(0));
					Assert.That(result.FileLength, Is.GreaterThan(0));
					Assert.That(result.EndDate, Is.GreaterThan(result.StartDate));

                }
            }
        }


    }
}
