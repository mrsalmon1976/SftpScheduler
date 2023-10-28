using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Security;
using SftpScheduler.BLL.Tests.Builders.Models;
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
            IHostValidator hostValidator = Substitute.For<IHostValidator>();
            var hostEntity = new HostEntityBuilder().WithRandomProperties().Build();

            hostValidator.Validate(hostEntity).Returns(new ValidationResult());

            UpdateHostCommand command = CreateCommand(hostValidator);
            command.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteNonQueryAsync(Arg.Any<string>(), hostEntity);

        }

        [Test]
        public void Execute_InvalidHost_ThrowsDataValidationException()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IHostValidator hostValidator = Substitute.For<IHostValidator>();
            var hostEntity = new HostEntityBuilder().WithRandomProperties().Build();
            hostValidator.Validate(Arg.Any<HostEntity>()).Returns(new ValidationResult(new string[] { "error" }));

            UpdateHostCommand updateHostCommand = CreateCommand(hostValidator);

            Assert.Throws<DataValidationException>(() => updateHostCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult());
            hostValidator.Received(1).Validate(hostEntity);
        }

        [Test]
        public void Execute_ValidHostPasswordLeftBlank_PasswordNotUpdated()
        {
            // setup
            IEncryptionProvider encryptionProvider = Substitute.For<IEncryptionProvider>();
            string executedSql = String.Empty;

            var hostEntity = new HostEntityBuilder().WithRandomProperties().WithPassword(String.Empty).Build();

            IDbContext dbContext = Substitute.For<IDbContext>();
            IHostValidator hostValidator = Substitute.For<IHostValidator>();
            hostValidator.Validate(hostEntity).Returns(new ValidationResult());

            dbContext.When(x => x.ExecuteNonQueryAsync(Arg.Any<string>(), Arg.Any<HostEntity>())).Do((ci) =>
            {
                executedSql = ci.ArgAt<string>(0);
            });

            // execute
            UpdateHostCommand updateHostCommand = CreateCommand(hostValidator, encryptionProvider);
            HostEntity result = updateHostCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult();

            // assert
            Assert.That(result, Is.Not.Null);
            encryptionProvider.DidNotReceive().Encrypt(Arg.Any<string>());

            Assert.That(executedSql.Contains("Password = Password"));
        }

        [Test]
        public void Execute_ValidHostPasswordChanged_PasswordEncryptedBeforeStorage()
        {
            // setup
            IEncryptionProvider encryptionProvider = Substitute.For<IEncryptionProvider>();
            string password = Guid.NewGuid().ToString();
            string executedSql = String.Empty;

            var hostEntity = new HostEntityBuilder().WithRandomProperties().WithPassword(password).Build();

            IDbContext dbContext = Substitute.For<IDbContext>();
            IHostValidator hostValidator = Substitute.For<IHostValidator>();
            hostValidator.Validate(hostEntity).Returns(new ValidationResult());

            dbContext.When(x => x.ExecuteNonQueryAsync(Arg.Any<string>(), Arg.Any<HostEntity>())).Do((ci) =>
            {
                executedSql = ci.ArgAt<string>(0);
            });

            // execute
            UpdateHostCommand updateHostCommand = CreateCommand(hostValidator, encryptionProvider);
            HostEntity result = updateHostCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult();

            // assert
            Assert.That(result, Is.Not.Null);
            encryptionProvider.Received(1).Encrypt(password);

            Assert.That(executedSql.Contains("Password = @Password"));
        }

        [Test]
        public void Execute_ValidHost_RemovesPasswordOnReturnValue()
        {
            // setup
            var hostEntity = new HostEntityBuilder().WithRandomProperties().Build();

            IDbContext dbContext = Substitute.For<IDbContext>();
            IHostValidator hostValidator = Substitute.For<IHostValidator>();
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
                IHostValidator hostValidator = Substitute.For<IHostValidator>();
                hostValidator.Validate(Arg.Any<HostEntity>()).Returns(new ValidationResult());

                UpdateHostCommand updateHostCommand = new UpdateHostCommand(new HostValidator(), new EncryptionProvider());
                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    DateTime dtBefore = DateTime.UtcNow;
                    var hostEntity = new HostEntityBuilder().WithRandomProperties().Build();

                    HostEntity result = updateHostCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult();

                    Assert.IsNotNull(result);
                    Assert.That(result.Name, Is.EqualTo(hostEntity.Name));
                    Assert.That(hostEntity.Created, Is.GreaterThanOrEqualTo(dtBefore));

                }
            }
        }

        private static UpdateHostCommand CreateCommand(IHostValidator hostValidator, IEncryptionProvider? encryptionProvider = null)
        {
            encryptionProvider = (encryptionProvider == null ? Substitute.For<IEncryptionProvider>() : encryptionProvider);
            return new UpdateHostCommand(hostValidator, encryptionProvider);
        }


    }
}
