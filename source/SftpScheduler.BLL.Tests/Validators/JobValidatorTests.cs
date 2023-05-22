using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Data;
using SftpScheduler.Common.IO;
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
            hostRepository.GetByIdAsync(dbContext, jobEntity.HostId).Returns(Task.FromResult(hostEntity));

            IDirectoryUtility directoryWrap = Substitute.For<IDirectoryUtility>();
            directoryWrap.Exists(jobEntity.LocalPath).Returns(true);

            JobValidator jobValidator = CreateJobValidator(hostRepository, directoryWrap);
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

            // execute
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);

            // assert
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }

        [Test]
        public void Validate_InValidNoHost_ExceptionThrown()
        {
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.HostId = 0;
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
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            HostEntity hostEntity = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            var directoryWrap = Substitute.For<IDirectoryUtility>();
            directoryWrap.Exists(Arg.Any<string>()).Returns(true);


            IDbContext dbContext = Substitute.For<IDbContext>();
            HostRepository hostRepository = Substitute.For<HostRepository>();
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            hostRepository.GetByIdAsync(dbContext, hostId).Returns(Task.FromResult(hostEntity));
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

            JobValidator jobValidator = CreateJobValidator(hostRepository, directoryWrap);
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
        public void Validate_LocalPathDoesNotExist_ExceptionThrown()
        {
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            IDbContext dbContext = Substitute.For<IDbContext>();
            IDirectoryUtility directoryWrap = Substitute.For<IDirectoryUtility>();
            directoryWrap.Exists(Arg.Any<string>()).Returns(false);
            JobValidator jobValidator = CreateJobValidator(Substitute.For<HostRepository>(), directoryWrap);
            var validationResult = jobValidator.Validate(dbContext, jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
            Assert.That(validationResult.ErrorMessages[0].Contains("local path does not exist", StringComparison.InvariantCultureIgnoreCase));
            directoryWrap.Received(1).Exists(jobEntity.LocalPath);
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

        [TestCase("")]
        [TestCase("  ")]
        [TestCase(null)]
        public void Validate_DeleteRemoteFalseAndNoRemoteArchiveFolderProvided_ExceptionThrown(string remoteArchiveFolder)
        {
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.DeleteAfterDownload = false;
            jobEntity.RemoteArchivePath = remoteArchiveFolder;

            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);

            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
            Assert.That(validationResult.ErrorMessages[0].Contains("Remote archive path must be supplied if deletion after download is not selected", StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void Validate_LocalCopyPathsExist_ValidatesCorrectly()
        {
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.LocalCopyPaths = "C:\\Temp\\Folder1;C:\\Temp\\Folder2";

            IDirectoryUtility dirWrap = Substitute.For<IDirectoryUtility>();
            dirWrap.Exists(jobEntity.LocalPath).Returns(true);
            dirWrap.Exists("C:\\Temp\\Folder1").Returns(true);
            dirWrap.Exists("C:\\Temp\\Folder2").Returns(true);

            JobValidator jobValidator = CreateJobValidator(Substitute.For<HostRepository>(), dirWrap);
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);

            Assert.That(validationResult.IsValid, Is.True);
            dirWrap.Received(1).Exists("C:\\Temp\\Folder1");
            dirWrap.Received(1).Exists("C:\\Temp\\Folder2");
            dirWrap.Received(3).Exists(Arg.Any<string>()); ;
        }

        [Test]
        public void Validate_LocalCopyPathDoesNotExist_ExceptionThrown()
        {
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.LocalCopyPaths = "C:\\Temp\\Folder1;C:\\Temp\\Folder2";

            IDirectoryUtility dirWrap = Substitute.For<IDirectoryUtility>();
            dirWrap.Exists(jobEntity.LocalPath).Returns(true);
            dirWrap.Exists("C:\\Temp\\Folder1").Returns(false);
            dirWrap.Exists("C:\\Temp\\Folder2").Returns(false);

            JobValidator jobValidator = CreateJobValidator(Substitute.For<HostRepository>(), dirWrap);
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);

            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(2));
            Assert.That(validationResult.ErrorMessages[0].Contains("Local copy path 'C:\\Temp\\Folder1' does not exist", StringComparison.InvariantCultureIgnoreCase));
        }


        [Test]
        public void Validate_InValidAllFields_ExceptionThrown()
        {
            JobEntity jobEntity = new JobEntity();

            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(7));
        }

        #region Private Methods

        // creates a validator which passes everything
        private JobValidator CreateJobValidator()
        {
            HostRepository hostRepository = Substitute.For<HostRepository>();
            hostRepository.GetByIdAsync(Arg.Any<IDbContext>(), Arg.Any<int>()).Returns(Task.FromResult(new HostEntity()));

            IDirectoryUtility dirWrap = Substitute.For<IDirectoryUtility>();
            dirWrap.Exists(Arg.Any<string>()).Returns(true);


            return CreateJobValidator(hostRepository, dirWrap);

        }

        private JobValidator CreateJobValidator(HostRepository hostRepository, IDirectoryUtility directoryWrap)
        {
            return new JobValidator(hostRepository, directoryWrap);

        }
        #endregion

    }

}
