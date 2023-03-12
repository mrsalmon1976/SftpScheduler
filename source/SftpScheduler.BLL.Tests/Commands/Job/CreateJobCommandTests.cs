using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Tests.Commands.Job
{
    [TestFixture]
    public class CreateJobCommandTests
    {

        [Test]
        public void Execute_ValidJob_ExecutesQuery()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            HostRepository hostRepository = Substitute.For<HostRepository>();
            JobValidator jobValidator = Substitute.For<JobValidator>(hostRepository);
            var jobEntity = EntityTestHelper.CreateJobEntity();

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            CreateJobCommand createJobCommand = new CreateJobCommand(jobValidator);
            createJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteNonQueryAsync(Arg.Any<string>(), jobEntity);
            dbContext.Received(1).ExecuteScalarAsync<int>(Arg.Any<string>());

        }


        [Test]
        public void Execute_InvalidJob_ThrowsDataValidationException()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            JobValidator jobValidator = Substitute.For<JobValidator>(Substitute.For<HostRepository>());
            var jobEntity = EntityTestHelper.CreateJobEntity();
            jobValidator.Validate(dbContext, Arg.Any<JobEntity>()).Returns(new ValidationResult(new string[] { "error" }));

            CreateJobCommand createJobCommand = new CreateJobCommand(jobValidator);

            Assert.Throws<DataValidationException>(() => createJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult());
            jobValidator.Received(1).Validate(dbContext, jobEntity);
        }

        [Test]
        public void Execute_ValidJob_PopulatesJobIdOnReturnValue()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            JobValidator jobValidator = Substitute.For<JobValidator>(Substitute.For<HostRepository>());

            var jobEntity = EntityTestHelper.CreateJobEntity();
            int newEntityId = Faker.RandomNumber.Next();

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());
            dbContext.ExecuteScalarAsync<int>(Arg.Any<string>()).Returns(newEntityId);

            CreateJobCommand createJobCommand = new CreateJobCommand(jobValidator);
            JobEntity result = createJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteScalarAsync<int>(Arg.Any<string>());
            Assert.That(result.Id, Is.EqualTo(newEntityId));
        }

        [Test]
        public void ExecuteAsync_Integration_ExecutesQueryWithoutError()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    CreateJobCommand createJobCommand = new CreateJobCommand(new JobValidator(new HostRepository()));
                    HostEntity host = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    DateTime dtBefore = DateTime.UtcNow;
                    JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
                    jobEntity.HostId = host.Id;

                    JobEntity result = createJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Name, jobEntity.Name);
                    Assert.That(jobEntity.Created, Is.GreaterThanOrEqualTo(dtBefore));

                }
            }
        }


    }
}
