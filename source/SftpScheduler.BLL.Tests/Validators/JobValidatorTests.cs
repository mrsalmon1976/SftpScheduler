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


            JobValidator jobValidator = CreateJobValidator(dbContext, hostRepository);
            var validationResult = jobValidator.Validate(jobEntity);
            Assert.That(validationResult.IsValid);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public void Validate_InValidObjectWithNoName_ExceptionThrown(string name)
        {
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.Name = name;
            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }

        [Test]
        public void Validate_InValidObjectWithNoHost_ExceptionThrown()
        {
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.HostId = null;
            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }

        [Test]
        public void Validate_InValidObjectWithHostThatDoesNotExist_ExceptionThrown()
        {
            int hostId = Faker.RandomNumber.Next(1, 100);
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.HostId = hostId;
            HostEntity hostEntity = null;

            IDbContext dbContext= Substitute.For<IDbContext>();
            HostRepository hostRepository = Substitute.For<HostRepository>();
            hostRepository.GetByIdAsync(dbContext, hostId).Returns(Task.FromResult(hostEntity));

            JobValidator jobValidator = CreateJobValidator(dbContext, hostRepository);
            var validationResult = jobValidator.Validate(jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }


        [Test]
        public void Validate_InValidObjectWithInvalidSchedule_ExceptionThrown()
        {
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.Schedule = "invalid";
            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }


        [Test]
        public void Validate_InValidObject_ExceptionThrown()
        {
            JobEntity jobEntity = new JobEntity();

            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(3));
        }

        #region Private Methods

        private JobValidator CreateJobValidator()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            HostRepository hostRepository = Substitute.For<HostRepository>();
            return CreateJobValidator(dbContext, hostRepository);

        }

        private JobValidator CreateJobValidator(IDbContext dbContext, HostRepository hostRepository)
        {
            return new JobValidator(dbContext, hostRepository);

        }
        #endregion

    }

}
