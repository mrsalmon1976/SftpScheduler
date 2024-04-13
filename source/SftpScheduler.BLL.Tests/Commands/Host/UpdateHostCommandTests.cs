using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Security;
using SftpScheduler.BLL.Services.Host;
using SftpScheduler.BLL.Validators;
using SftpScheduler.Test.Common;

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
            var hostEntity = new SubstituteBuilder<HostEntity>().WithRandomProperties().Build();
            string userName = Guid.NewGuid().ToString();   

            hostValidator.Validate(hostEntity).Returns(new ValidationResult());

            UpdateHostCommand command = CreateCommand(hostValidator: hostValidator);
            command.ExecuteAsync(dbContext, hostEntity, userName).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteNonQueryAsync(Arg.Any<string>(), hostEntity);

        }

        [Test]
        public void Execute_ValidHost_CreatesAuditLogs()
        {
            // setup 
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IHostValidator hostValidator = Substitute.For<IHostValidator>();
            var hostEntity = new SubstituteBuilder<HostEntity>().WithRandomProperties().Build();
            hostValidator.Validate(hostEntity).Returns(new ValidationResult());
            string userName = Guid.NewGuid().ToString();

            HostAuditLogEntity hostAuditLogEntity1 = new SubstituteBuilder<HostAuditLogEntity>().WithRandomProperties().WithProperty(x => x.HostId, hostEntity.Id).Build();
            HostAuditLogEntity hostAuditLogEntity2 = new SubstituteBuilder<HostAuditLogEntity>().WithRandomProperties().WithProperty(x => x.HostId, hostEntity.Id).Build();
            IEnumerable<HostAuditLogEntity> auditLogEntities = new HostAuditLogEntity[] { hostAuditLogEntity1, hostAuditLogEntity2 };
            IHostAuditService hostAuditService = new SubstituteBuilder<IHostAuditService>().Build();
            hostAuditService.CompareHosts(Arg.Any<HostEntity>(), Arg.Any<HostEntity>(), userName).Returns(auditLogEntities);

            // execute
            UpdateHostCommand command = CreateCommand(hostValidator: hostValidator, hostAuditService: hostAuditService);
            command.ExecuteAsync(dbContext, hostEntity, userName).GetAwaiter().GetResult();

            // assert
            dbContext.Received(1).ExecuteNonQueryAsync(Arg.Any<string>(), hostEntity);
            dbContext.Received(1).ExecuteNonQueryAsync(Arg.Any<string>(), hostAuditLogEntity1);
            dbContext.Received(1).ExecuteNonQueryAsync(Arg.Any<string>(), hostAuditLogEntity2);

        }

        [Test]
        public void Execute_InvalidHost_ThrowsDataValidationException()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IHostValidator hostValidator = Substitute.For<IHostValidator>();
            string userName = Guid.NewGuid().ToString();
            var hostEntity = new SubstituteBuilder<HostEntity>().WithRandomProperties().Build();
            hostValidator.Validate(Arg.Any<HostEntity>()).Returns(new ValidationResult(new string[] { "error" }));

            UpdateHostCommand updateHostCommand = CreateCommand(hostValidator: hostValidator);

            Assert.Throws<DataValidationException>(() => updateHostCommand.ExecuteAsync(dbContext, hostEntity, userName).GetAwaiter().GetResult());
            hostValidator.Received(1).Validate(hostEntity);
        }

        [Test]
        public void Execute_ValidHostPasswordLeftBlank_PasswordNotUpdated()
        {
            // setup
            IEncryptionProvider encryptionProvider = Substitute.For<IEncryptionProvider>();
            string executedSql = String.Empty;

            string userName = Guid.NewGuid().ToString();
            var hostEntity = new SubstituteBuilder<HostEntity>().WithRandomProperties().WithProperty(x => x.Password, String.Empty).Build();

            IDbContext dbContext = Substitute.For<IDbContext>();
            IHostValidator hostValidator = Substitute.For<IHostValidator>();
            hostValidator.Validate(hostEntity).Returns(new ValidationResult());

            dbContext.When(x => x.ExecuteNonQueryAsync(Arg.Any<string>(), Arg.Any<HostEntity>())).Do((ci) =>
            {
                executedSql = ci.ArgAt<string>(0);
            });

            // execute
            UpdateHostCommand updateHostCommand = CreateCommand(hostValidator: hostValidator, encryptionProvider: encryptionProvider);
            HostEntity result = updateHostCommand.ExecuteAsync(dbContext, hostEntity, userName).GetAwaiter().GetResult();

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

            string userName = Guid.NewGuid().ToString();
            var hostEntity = new SubstituteBuilder<HostEntity>().WithRandomProperties().WithProperty(x => x.Password, password).Build();

            IDbContext dbContext = Substitute.For<IDbContext>();
            IHostValidator hostValidator = Substitute.For<IHostValidator>();
            hostValidator.Validate(hostEntity).Returns(new ValidationResult());

            dbContext.When(x => x.ExecuteNonQueryAsync(Arg.Any<string>(), Arg.Any<HostEntity>())).Do((ci) =>
            {
                executedSql = ci.ArgAt<string>(0);
            });

            // execute
            UpdateHostCommand updateHostCommand = CreateCommand(hostValidator: hostValidator, encryptionProvider: encryptionProvider);
            HostEntity result = updateHostCommand.ExecuteAsync(dbContext, hostEntity, userName).GetAwaiter().GetResult();

            // assert
            Assert.That(result, Is.Not.Null);
            encryptionProvider.Received(1).Encrypt(password);

            Assert.That(executedSql.Contains("Password = @Password"));
        }

        [Test]
        public void Execute_ValidHost_RemovesPasswordOnReturnValue()
        {
            // setup
            var hostEntity = new SubstituteBuilder<HostEntity>().WithRandomProperties().Build();

            string userName = Guid.NewGuid().ToString();
            IDbContext dbContext = Substitute.For<IDbContext>();
            IHostValidator hostValidator = Substitute.For<IHostValidator>();
            hostValidator.Validate(hostEntity).Returns(new ValidationResult());

            // execute
            UpdateHostCommand updateHostCommand = CreateCommand(hostValidator: hostValidator);
            HostEntity result = updateHostCommand.ExecuteAsync(dbContext, hostEntity, userName).GetAwaiter().GetResult();

            // assert
            Assert.That(result.Password, Is.Empty);
        }

        [Test]
        public void Execute_Integration_ExecutesQueryWithoutError()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();
                string userName = Guid.NewGuid().ToString();
                IHostValidator hostValidator = Substitute.For<IHostValidator>();
                hostValidator.Validate(Arg.Any<HostEntity>()).Returns(new ValidationResult());

                UpdateHostCommand updateHostCommand = new UpdateHostCommand(new HostRepository(), new HostValidator(), new EncryptionProvider(), new HostAuditService());
                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    DateTime dtBefore = DateTime.UtcNow;
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);
                    hostEntity.Name = Guid.NewGuid().ToString();

                    HostEntity result = updateHostCommand.ExecuteAsync(dbContext, hostEntity, userName).GetAwaiter().GetResult();

                    Assert.IsNotNull(result);
                    Assert.That(result.Name, Is.EqualTo(hostEntity.Name));
                    Assert.That(hostEntity.Created, Is.GreaterThanOrEqualTo(dtBefore));

                }
            }
        }

        private static UpdateHostCommand CreateCommand(HostRepository? hostRepository = null, IHostValidator? hostValidator = null, IEncryptionProvider? encryptionProvider = null, IHostAuditService? hostAuditService = null)
        {
            hostRepository = (hostRepository == null ? Substitute.For<HostRepository>() : hostRepository);
            hostValidator = (hostValidator == null ? Substitute.For<IHostValidator>() : hostValidator);
            encryptionProvider = (encryptionProvider == null ? Substitute.For<IEncryptionProvider>() : encryptionProvider);
            hostAuditService = (hostAuditService == null ? Substitute.For<IHostAuditService>() : hostAuditService);
            return new UpdateHostCommand(hostRepository, hostValidator, encryptionProvider, hostAuditService);
        }


    }
}
