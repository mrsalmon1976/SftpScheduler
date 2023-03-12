using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Tests.Validators
{
    [TestFixture]
    public class JobValidatorTests
    {
        [Test]
        public void Validate_ValidObject_NoValidationExceptionThrown()
        {
            HostEntity hostEntity = EntityTestHelper.CreateHostEntity();
            hostEntity.Id = Faker.RandomNumber.Next(1, 100);
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.HostId = hostEntity.Id;

            IDbContext dbContext = Substitute.For<IDbContext>();
            HostRepository hostRepository = Substitute.For<HostRepository>();
            hostRepository.GetByIdAsync(dbContext, jobEntity.HostId.Value).Returns(Task.FromResult(hostEntity));


            JobValidator jobValidator = CreateJobValidator(hostRepository);
            var validationResult = jobValidator.Validate(dbContext, jobEntity);
            Assert.That(validationResult.IsValid);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public void Validate_InvalidNoName_ExceptionThrown(string name)
        {
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.Name = name;
            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }

        [Test]
        public void Validate_InValidNoHost_ExceptionThrown()
        {
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.HostId = null;
            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }

        [Test]
        public void Validate_InValidHostDoesNotExist_ExceptionThrown()
        {
            int hostId = Faker.RandomNumber.Next(1, 100);
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.HostId = hostId;
            HostEntity hostEntity = null;

            IDbContext dbContext= Substitute.For<IDbContext>();
            HostRepository hostRepository = Substitute.For<HostRepository>();
            hostRepository.GetByIdAsync(dbContext, hostId).Returns(Task.FromResult(hostEntity));

            JobValidator jobValidator = CreateJobValidator(hostRepository);
            var validationResult = jobValidator.Validate(dbContext, jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }

        [TestCase(0)]
        [TestCase(3)]
        public void Validate_InValidType_ExceptionThrown(int jobType)
        {
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.Type = (JobType)jobType;
            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
            Assert.That(validationResult.ErrorMessages[0].Contains("job type must be 1", StringComparison.InvariantCultureIgnoreCase));
        }


        [Test]
        public void Validate_InValidSchedule_ExceptionThrown()
        {
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.Schedule = "invalid";
            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }

        [Test]
        public void Validate_InValidLocalPath_ExceptionThrown()
        {
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.LocalPath = "";
            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
            Assert.That(validationResult.ErrorMessages[0].Contains("local path is required", StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void Validate_InValidRemotePath_ExceptionThrown()
        {
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.RemotePath = "";
            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
            Assert.That(validationResult.ErrorMessages[0].Contains("remote path is required", StringComparison.InvariantCultureIgnoreCase));
        }


        [Test]
        public void Validate_InValidAllFields_ExceptionThrown()
        {
            JobEntity jobEntity = new JobEntity();

            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(6));
        }

        #region Private Methods

        private JobValidator CreateJobValidator()
        {
            HostRepository hostRepository = Substitute.For<HostRepository>();
            return CreateJobValidator(hostRepository);

        }

        private JobValidator CreateJobValidator(HostRepository hostRepository)
        {
            return new JobValidator(hostRepository);

        }
        #endregion

    }

}
