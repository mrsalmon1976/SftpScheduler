using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Security;
using SftpScheduler.BLL.Tests.Builders.Data;
using SftpScheduler.BLL.Tests.Builders.Models;
using SftpScheduler.BLL.Validators;

namespace SftpScheduler.BLL.Tests.Commands.Host
{
    [TestFixture]
    public class CreateHostCommandTests
    {
        [Test]
        public void Execute_ValidHost_ExecutesQuery()
        {
            // setup
            IDbContext dbContext = new DbContextBuilder().Build();
            IHostValidator hostValidator = Substitute.For<IHostValidator>();

            var hostEntity = new HostEntityBuilder().WithRandomProperties().Build();
            hostValidator.Validate(hostEntity).Returns(new ValidationResult());

            // execute
            CreateHostCommand createHostCommand = new CreateHostCommand(hostValidator, Substitute.For<IEncryptionProvider>());
            createHostCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult();

            // assert
            dbContext.Received(1).ExecuteNonQueryAsync(Arg.Any<string>(), hostEntity);
            dbContext.Received(1).ExecuteScalarAsync<int>(Arg.Any<string>());

        }

        [Test]
        public void Execute_InvalidHost_ThrowsDataValidationException()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IHostValidator hostValidator = Substitute.For<IHostValidator>();
            var hostEntity = new HostEntityBuilder().WithRandomProperties().Build();
            hostValidator.Validate(Arg.Any<HostEntity>()).Returns(new ValidationResult(new string[] { "error" }));

            CreateHostCommand createHostCommand = new CreateHostCommand(hostValidator, Substitute.For<IEncryptionProvider>());

            Assert.Throws<DataValidationException>(() => createHostCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult());
            hostValidator.Received(1).Validate(hostEntity);
        }

        [Test]
        public void Execute_ValidHost_PopulatesHostIdOnReturnValue()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IHostValidator hostValidator = Substitute.For<IHostValidator>();

            var hostEntity = new HostEntityBuilder().WithRandomProperties().Build();
            int newEntityId = Faker.RandomNumber.Next();

            hostValidator.Validate(hostEntity).Returns(new ValidationResult());
            dbContext.ExecuteScalarAsync<int>(Arg.Any<string>()).Returns(newEntityId);

            CreateHostCommand createHostCommand = new CreateHostCommand(hostValidator, Substitute.For<IEncryptionProvider>());
            HostEntity result = createHostCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteScalarAsync<int>(Arg.Any<string>());
            Assert.That(result.Id, Is.EqualTo(newEntityId));
        }

        [Test]
        public void Execute_ValidHost_PasswordEncryptedBeforeStorage()
        {
            // setup
            IEncryptionProvider encryptionProvider = Substitute.For<IEncryptionProvider>();
            string password = Guid.NewGuid().ToString();

            var hostEntity = new HostEntityBuilder().WithRandomProperties().WithPassword(password).Build();

            IDbContext dbContext = Substitute.For<IDbContext>();
            IHostValidator hostValidator = Substitute.For<IHostValidator>();
            hostValidator.Validate(hostEntity).Returns(new ValidationResult());

            // execute
            CreateHostCommand createHostCommand = new CreateHostCommand(hostValidator, encryptionProvider);
            HostEntity result = createHostCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult();

            // assert
            Assert.That(result, Is.Not.Null);
            encryptionProvider.Received(1).Encrypt(password);
        }

        [Test]
        public void Execute_ValidHost_RemovesPasswordOnReturnValue()
        {
            // setup
            IEncryptionProvider encryptionProvider = Substitute.For<IEncryptionProvider>();

            var hostEntity = new HostEntityBuilder().WithRandomProperties().Build();

            IDbContext dbContext = Substitute.For<IDbContext>();
            IHostValidator hostValidator = Substitute.For<IHostValidator>();
            hostValidator.Validate(hostEntity).Returns(new ValidationResult());

            // execute
            CreateHostCommand createHostCommand = new CreateHostCommand(hostValidator, encryptionProvider);
            HostEntity result = createHostCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult();

            // assert
            Assert.That(result.Password, Is.Empty);
        }

        [Test]
        public void Execute_Integration_ExecutesQueryWithoutError()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                CreateHostCommand createHostCommand = new CreateHostCommand(new HostValidator(), new EncryptionProvider());
                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    DateTime dtBefore = DateTime.UtcNow;
                    HostEntity hostEntity = new HostEntityBuilder().WithRandomProperties().Build();

                    HostEntity result = createHostCommand.ExecuteAsync(dbContext, hostEntity).GetAwaiter().GetResult();

                    Assert.IsNotNull(result);
                    Assert.That(result.Name, Is.EqualTo(hostEntity.Name));
                    Assert.That(hostEntity.Created, Is.GreaterThanOrEqualTo(dtBefore));

                }
            }
        }


    }
}
