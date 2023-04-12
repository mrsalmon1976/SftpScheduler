using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Security;
using SftpScheduler.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Tests.Commands.Host
{
    [TestFixture]
    public class CreateHostCommandTests
    {
        [Test]
        public void Execute_ValidHost_ExecutesQuery()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            HostValidator hostValidator = Substitute.For<HostValidator>();
            var hostEntity = EntityTestHelper.CreateHostEntity();

            hostValidator.Validate(hostEntity).Returns(new ValidationResult());

            CreateHostCommand createJobCommand = new CreateHostCommand(hostValidator, Substitute.For<IPasswordProvider>());
            createJobCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteNonQueryAsync(Arg.Any<string>(), hostEntity);
            dbContext.Received(1).ExecuteScalarAsync<int>(Arg.Any<string>());

        }

        [Test]
        public void Execute_InvalidHost_ThrowsDataValidationException()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            HostValidator hostValidator = Substitute.For<HostValidator>();
            var hostEntity = EntityTestHelper.CreateHostEntity();
            hostValidator.Validate(Arg.Any<HostEntity>()).Returns(new ValidationResult(new string[] { "error" }));

            CreateHostCommand createJobCommand = new CreateHostCommand(hostValidator, Substitute.For<IPasswordProvider>());

            Assert.Throws<DataValidationException>(() => createJobCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult());
            hostValidator.Received(1).Validate(hostEntity);
        }

        [Test]
        public void Execute_ValidHost_PopulatesHostIdOnReturnValue()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            HostValidator hostValidator = Substitute.For<HostValidator>();

            var hostEntity = EntityTestHelper.CreateHostEntity();
            int newEntityId = Faker.RandomNumber.Next();

            hostValidator.Validate(hostEntity).Returns(new ValidationResult());
            dbContext.ExecuteScalarAsync<int>(Arg.Any<string>()).Returns(newEntityId);

            CreateHostCommand createJobCommand = new CreateHostCommand(hostValidator, Substitute.For<IPasswordProvider>());
            HostEntity result = createJobCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteScalarAsync<int>(Arg.Any<string>());
            Assert.That(result.Id, Is.EqualTo(newEntityId));
        }

        [Test]
        public void Execute_ValidHost_PasswordEncryptedBeforeStorage()
        {
            // setup
            IPasswordProvider passwordProvider = Substitute.For<IPasswordProvider>();
            string password = Guid.NewGuid().ToString();

            var hostEntity = EntityTestHelper.CreateHostEntity();
            hostEntity.Password = password;

            IDbContext dbContext = Substitute.For<IDbContext>();
            HostValidator hostValidator = Substitute.For<HostValidator>();
            hostValidator.Validate(hostEntity).Returns(new ValidationResult());

            // execute
            CreateHostCommand createJobCommand = new CreateHostCommand(hostValidator, passwordProvider);
            HostEntity result = createJobCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult();

            // assert
            Assert.That(result, Is.Not.Null);
            passwordProvider.Received(1).Encrypt(password);
        }

        [Test]
        public void Execute_ValidHost_RemovesPasswordOnReturnValue()
        {
            // setup
            IPasswordProvider passwordProvider = Substitute.For<IPasswordProvider>();

            var hostEntity = EntityTestHelper.CreateHostEntity();

            IDbContext dbContext = Substitute.For<IDbContext>();
            HostValidator hostValidator = Substitute.For<HostValidator>();
            hostValidator.Validate(hostEntity).Returns(new ValidationResult());

            // execute
            CreateHostCommand createJobCommand = new CreateHostCommand(hostValidator, passwordProvider);
            HostEntity result = createJobCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult();

            // assert
            Assert.That(result.Password, Is.Empty);
        }

        [Test]
        public void Execute_Integration_ExecutesQueryWithoutError()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();
                HostValidator hostValidator = Substitute.For<HostValidator>();
                hostValidator.Validate(Arg.Any<HostEntity>()).Returns(new ValidationResult());

                CreateHostCommand createHostCommand = new CreateHostCommand(new HostValidator(), new PasswordProvider("test"));
                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    DateTime dtBefore = DateTime.UtcNow;
                    HostEntity hostEntity = EntityTestHelper.CreateHostEntity();

                    HostEntity result = createHostCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult();

                    Assert.IsNotNull(result);
                    Assert.That(result.Name, Is.EqualTo(hostEntity.Name));
                    Assert.That(hostEntity.Created, Is.GreaterThanOrEqualTo(dtBefore));

                }
            }
        }


    }
}
