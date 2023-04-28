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
    public class UpdateHostCommandTests
    {
        [Test]
        public void Execute_ValidHost_ExecutesQuery()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            HostValidator hostValidator = Substitute.For<HostValidator>();
            var hostEntity = EntityTestHelper.CreateHostEntity();

            hostValidator.Validate(hostEntity).Returns(new ValidationResult());

            UpdateHostCommand command = CreateCommand(hostValidator);
            command.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteNonQueryAsync(Arg.Any<string>(), hostEntity);

        }

        [Test]
        public void Execute_InvalidHost_ThrowsDataValidationException()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            HostValidator hostValidator = Substitute.For<HostValidator>();
            var hostEntity = EntityTestHelper.CreateHostEntity();
            hostValidator.Validate(Arg.Any<HostEntity>()).Returns(new ValidationResult(new string[] { "error" }));

            UpdateHostCommand updateHostCommand = CreateCommand(hostValidator);

            Assert.Throws<DataValidationException>(() => updateHostCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult());
            hostValidator.Received(1).Validate(hostEntity);
        }

        [Test]
        public void Execute_ValidHostPasswordLeftBlank_PasswordNotUpdated()
        {
            // setup
            IPasswordProvider passwordProvider = Substitute.For<IPasswordProvider>();
            string executedSql = String.Empty;

            var hostEntity = EntityTestHelper.CreateHostEntity();
            hostEntity.Password = String.Empty;

            IDbContext dbContext = Substitute.For<IDbContext>();
            HostValidator hostValidator = Substitute.For<HostValidator>();
            hostValidator.Validate(hostEntity).Returns(new ValidationResult());

            dbContext.When(x => x.ExecuteNonQueryAsync(Arg.Any<string>(), Arg.Any<HostEntity>())).Do((ci) =>
            {
                executedSql = ci.ArgAt<string>(0);
            });

            // execute
            UpdateHostCommand updateHostCommand = CreateCommand(hostValidator, passwordProvider);
            HostEntity result = updateHostCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult();

            // assert
            Assert.That(result, Is.Not.Null);
            passwordProvider.DidNotReceive().Encrypt(Arg.Any<string>());

            Assert.That(executedSql.Contains("Password = Password"));
        }

        [Test]
        public void Execute_ValidHostPasswordChanged_PasswordEncryptedBeforeStorage()
        {
            // setup
            IPasswordProvider passwordProvider = Substitute.For<IPasswordProvider>();
            string password = Guid.NewGuid().ToString();
            string executedSql = String.Empty;

            var hostEntity = EntityTestHelper.CreateHostEntity();
            hostEntity.Password = password;

            IDbContext dbContext = Substitute.For<IDbContext>();
            HostValidator hostValidator = Substitute.For<HostValidator>();
            hostValidator.Validate(hostEntity).Returns(new ValidationResult());

            dbContext.When(x => x.ExecuteNonQueryAsync(Arg.Any<string>(), Arg.Any<HostEntity>())).Do((ci) =>
            {
                executedSql = ci.ArgAt<string>(0);
            });

            // execute
            UpdateHostCommand updateHostCommand = CreateCommand(hostValidator, passwordProvider);
            HostEntity result = updateHostCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult();

            // assert
            Assert.That(result, Is.Not.Null);
            passwordProvider.Received(1).Encrypt(password);

            Assert.That(executedSql.Contains("Password = @Password"));
        }

        [Test]
        public void Execute_ValidHost_RemovesPasswordOnReturnValue()
        {
            // setup
            var hostEntity = EntityTestHelper.CreateHostEntity();

            IDbContext dbContext = Substitute.For<IDbContext>();
            HostValidator hostValidator = Substitute.For<HostValidator>();
            hostValidator.Validate(hostEntity).Returns(new ValidationResult());

            // execute
            UpdateHostCommand updateHostCommand = CreateCommand(hostValidator);
            HostEntity result = updateHostCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult();

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

                UpdateHostCommand updateHostCommand = new UpdateHostCommand(new HostValidator(), new PasswordProvider("test"));
                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    DateTime dtBefore = DateTime.UtcNow;
                    HostEntity hostEntity = EntityTestHelper.CreateHostEntity();

                    HostEntity result = updateHostCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult();

                    Assert.IsNotNull(result);
                    Assert.That(result.Name, Is.EqualTo(hostEntity.Name));
                    Assert.That(hostEntity.Created, Is.GreaterThanOrEqualTo(dtBefore));

                }
            }
        }

        private static UpdateHostCommand CreateCommand(HostValidator hostValidator, IPasswordProvider? passwordProvider = null)
        {
            passwordProvider = (passwordProvider == null ? Substitute.For<IPasswordProvider>() : passwordProvider);
            return new UpdateHostCommand(hostValidator, passwordProvider);
        }


    }
}
