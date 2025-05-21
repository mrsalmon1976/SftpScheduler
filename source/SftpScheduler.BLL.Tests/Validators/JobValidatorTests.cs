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
using System.Data;
using SftpScheduler.Test.Common;

namespace SftpScheduler.BLL.Tests.Validators
{
    [TestFixture]
    public class JobValidatorTests
    {
        [Test]
        public void Validate_ValidObject_NoValidationExceptionThrown()
        {
            int hostId = Faker.RandomNumber.Next(1, 100);
            var hostEntity = new SubstituteBuilder<HostEntity>().WithRandomProperties().WithProperty(x => x.Id, hostId).Build();
            JobEntity jobEntity = CreateValidJobEntityBuilder().WithProperty(x => x.HostId, hostId).Build();

            IDbContext dbContext = Substitute.For<IDbContext>();
            HostRepository hostRepository = Substitute.For<HostRepository>();
            hostRepository.GetByIdAsync(dbContext, jobEntity.HostId).Returns(Task.FromResult(hostEntity));

            IDirectoryUtility directoryWrap = Substitute.For<IDirectoryUtility>();
            directoryWrap.Exists(jobEntity.LocalPath).Returns(true);

            JobValidator jobValidator = CreateJobValidator(hostRepository, Substitute.For<JobRepository>(),directoryWrap);
            var validationResult = jobValidator.Validate(dbContext, jobEntity);
            Assert.That(validationResult.IsValid);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public void Validate_InvalidNoName_ExceptionThrown(string name)
        {
            JobEntity jobEntity = CreateValidJobEntityBuilder().WithProperty(x => x.Name, name).Build();
            JobValidator jobValidator = CreateJobValidator();

            // execute
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);

            // assert
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }

        [Test]
        public void Validate_InvalidNoHost_ExceptionThrown()
        {
            JobEntity jobEntity = CreateValidJobEntityBuilder().WithProperty(x => x.HostId, 0).Build();
            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }

        [Test]
        public void Validate_InvalidHostDoesNotExist_ExceptionThrown()
        {
            int hostId = Faker.RandomNumber.Next(1, 100);
            JobEntity jobEntity = CreateValidJobEntityBuilder().WithProperty(x => x.HostId, hostId).Build();
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

            JobValidator jobValidator = CreateJobValidator(hostRepository, Substitute.For<JobRepository>(), directoryWrap);
            var validationResult = jobValidator.Validate(dbContext, jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }

        [TestCase(0)]
        [TestCase(3)]
        public void Validate_InvalidType_ExceptionThrown(int jobType)
        {
            JobEntity jobEntity = CreateValidJobEntityBuilder().WithProperty(x => x.Type, (JobType)jobType).Build();
            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
            Assert.That(validationResult.ErrorMessages[0].Contains("job type must be 1", StringComparison.InvariantCultureIgnoreCase));
        }


        [Test]
        public void Validate_InvalidSchedule_ExceptionThrown()
        {
            JobEntity jobEntity = CreateValidJobEntityBuilder().WithProperty(x => x.Schedule, "invalid").Build();
            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }

        [Test]
        public void Validate_InvalidLocalPath_ExceptionThrown()
        {
            JobEntity jobEntity = CreateValidJobEntityBuilder().WithProperty(x => x.LocalPath, "").Build();
            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
            Assert.That(validationResult.ErrorMessages[0].Contains("local path is required", StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void Validate_InvalidLocalArchivePath_ExceptionThrown()
        {
            // setup
            string localArchivePath = $"C:\\{RandomData.String}";
            JobEntity jobEntity = CreateValidJobEntityBuilder().WithProperty(x => x.LocalArchivePath, localArchivePath).Build();

            IDirectoryUtility dirUtility = Substitute.For<IDirectoryUtility>();
            dirUtility.Exists(localArchivePath).Returns(false);
            dirUtility.Exists(jobEntity.LocalPath).Returns(true);

            JobValidator jobValidator = CreateJobValidator(dirUtility: dirUtility);

            // act
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);

            // assert
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
            Assert.That(validationResult.ErrorMessages[0].Contains("local archive path does not exist", StringComparison.InvariantCultureIgnoreCase));

            dirUtility.Received(1).Exists(localArchivePath);
        }

        [Test]
        public void Validate_LocalPathDoesNotExist_ExceptionThrown()
        {
            JobEntity jobEntity = CreateValidJobEntityBuilder().Build();
            IDbContext dbContext = Substitute.For<IDbContext>();
            IDirectoryUtility directoryWrap = Substitute.For<IDirectoryUtility>();
            directoryWrap.Exists(Arg.Any<string>()).Returns(false);
            JobValidator jobValidator = CreateJobValidator(Substitute.For<HostRepository>(), Substitute.For<JobRepository>(), directoryWrap);
            var validationResult = jobValidator.Validate(dbContext, jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
            Assert.That(validationResult.ErrorMessages[0].Contains("local path does not exist", StringComparison.InvariantCultureIgnoreCase));
            directoryWrap.Received(1).Exists(jobEntity.LocalPath);
        }

        [Test]
        public void Validate_InValidRemotePath_ExceptionThrown()
        {
            JobEntity jobEntity = CreateValidJobEntityBuilder()
                .WithProperty(x => x.RemotePath, "")
                .Build();

            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
            Assert.That(validationResult.ErrorMessages[0].Contains("remote path is required", StringComparison.InvariantCultureIgnoreCase));
        }

        [TestCase("")]
        [TestCase("  ")]
        [TestCase(null)]
        public void Validate_DownloadJobAndDeleteRemoteFalseAndNoRemoteArchiveFolderProvided_ExceptionThrown(string remoteArchiveFolder)
        {
            JobEntity jobEntity = CreateValidJobEntityBuilder()
                .WithProperty(x => x.DeleteAfterDownload, false)
                .WithProperty(x => x.RemoteArchivePath, remoteArchiveFolder)
                .WithProperty(x => x.Type, JobType.Download)
                .Build();

            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);

            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
            Assert.That(validationResult.ErrorMessages[0].Contains("Remote archive path must be supplied if deletion after download is not selected", StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void Validate_UploadloadJobAndDeleteRemoteFalseAndNoRemoteArchiveFolderProvided_ValidatesCorrectly()
        {
            JobEntity jobEntity = CreateValidJobEntityBuilder()
                .WithProperty(x => x.DeleteAfterDownload, false)
                .WithProperty(x => x.RemoteArchivePath, String.Empty)
                .WithProperty(x => x.Type, JobType.Upload)
                .Build();

            JobValidator jobValidator = CreateJobValidator();
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);

            Assert.That(validationResult.IsValid, Is.True);
        }

        [Test]
        public void Validate_LocalCopyPathsExist_ValidatesCorrectly()
        {
            string localCopyPaths = "C:\\Temp\\Folder1;C:\\Temp\\Folder2";
            JobEntity jobEntity = CreateValidJobEntityBuilder()
                .WithProperty(x => x.LocalCopyPaths, localCopyPaths)
                .Build();
            jobEntity.LocalCopyPathsAsEnumerable().Returns(localCopyPaths.Split(';'));

            IDirectoryUtility dirWrap = Substitute.For<IDirectoryUtility>();
            dirWrap.Exists(jobEntity.LocalPath).Returns(true);
            dirWrap.Exists("C:\\Temp\\Folder1").Returns(true);
            dirWrap.Exists("C:\\Temp\\Folder2").Returns(true);

            JobValidator jobValidator = CreateJobValidator(Substitute.For<HostRepository>(), Substitute.For<JobRepository>(), dirWrap);
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);

            Assert.That(validationResult.IsValid, Is.True);
            dirWrap.Received(1).Exists("C:\\Temp\\Folder1");
            dirWrap.Received(1).Exists("C:\\Temp\\Folder2");
            dirWrap.Received(3).Exists(Arg.Any<string>()); ;
        }

        [Test]
        public void Validate_LocalCopyPathDoesNotExist_ExceptionThrown()
        {
            string localCopyPaths = "C:\\Temp\\Folder1;C:\\Temp\\Folder2";
            JobEntity jobEntity = CreateValidJobEntityBuilder()
                .WithProperty(x => x.LocalCopyPaths, localCopyPaths)
                .Build();
            jobEntity.LocalCopyPathsAsEnumerable().Returns(localCopyPaths.Split(';'));

            IDirectoryUtility dirWrap = Substitute.For<IDirectoryUtility>();
            dirWrap.Exists(jobEntity.LocalPath).Returns(true);
            dirWrap.Exists("C:\\Temp\\Folder1").Returns(false);
            dirWrap.Exists("C:\\Temp\\Folder2").Returns(false);

            JobValidator jobValidator = CreateJobValidator(Substitute.For<HostRepository>(), Substitute.For<JobRepository>(), dirWrap);
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), jobEntity);

            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(2));
            Assert.That(validationResult.ErrorMessages[0].Contains("Local copy path 'C:\\Temp\\Folder1' does not exist", StringComparison.InvariantCultureIgnoreCase));
        }

        [TestCase("", "*.*")]
        [TestCase(null, "*.*")]
        [TestCase("*.*", "")]
        [TestCase("*.*", null)]
        [TestCase("*.txt", "*.txt")]
        public void Validate_DownloadWithDuplicateHostAndRemotePathAndFileMask_ExceptionThrown(string existingFileMask, string newFileMask)
        {
            // setup
            int hostId = new Random().Next(1, 100);
            string remotePath = "/" + Guid.NewGuid().ToString() + "/";

            JobEntity existingJobEntity = CreateValidJobEntityBuilder()
                .WithProperty(x => x.HostId, hostId)
                .WithProperty(x => x.RemotePath, remotePath)
                .WithProperty(x => x.Type, JobType.Download)
                .WithProperty(x => x.FileMask, existingFileMask)
                .Build();

            JobEntity newJobEntity = CreateValidJobEntityBuilder()
                .WithProperty(x => x.HostId, hostId)
                .WithProperty(x => x.RemotePath, remotePath)
                .WithProperty(x => x.Type, JobType.Download)
                .WithProperty(x => x.FileMask, newFileMask)
                .Build();

            JobRepository jobRepo = new SubstituteBuilder<JobRepository>().Build();
            jobRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(new JobEntity[] { existingJobEntity });

            // execute
            JobValidator jobValidator = CreateJobValidator(jobRepo: jobRepo);
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), newJobEntity);

            // assert
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
            Assert.That(validationResult.ErrorMessages[0].Contains("Download job clashes with existing job", StringComparison.InvariantCultureIgnoreCase));
        }

        [TestCase("", "*.*")]
        [TestCase(null, "*.*")]
        [TestCase("*.*", "")]
        [TestCase("*.*", null)]
        [TestCase("*.txt", "*.txt")]
        public void Validate_UploadWithDuplicateLocalPathAndFileMask_ExceptionThrown(string existingFileMask, string newFileMask)
        {
            // setup
            int hostId = new Random().Next(1, 100);
            string localPath = "\\\\myserver\\" + Guid.NewGuid().ToString() + "\\";

            JobEntity existingJobEntity = CreateValidJobEntityBuilder()
                .WithProperty(x => x.LocalPath, localPath)
                .WithProperty(x => x.Type, JobType.Upload)
                .WithProperty(x => x.FileMask, existingFileMask)
                .Build();

            JobEntity newJobEntity = CreateValidJobEntityBuilder()
                .WithProperty(x => x.LocalPath, localPath)
                .WithProperty(x => x.Type, JobType.Upload)
                .WithProperty(x => x.FileMask, newFileMask)
                .Build();

            JobRepository jobRepo = new SubstituteBuilder<JobRepository>().Build();
            jobRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(new JobEntity[] { existingJobEntity });

            // execute
            JobValidator jobValidator = CreateJobValidator(jobRepo: jobRepo);
            var validationResult = jobValidator.Validate(Substitute.For<IDbContext>(), newJobEntity);

            // assert
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
            Assert.That(validationResult.ErrorMessages[0].Contains("Upload job clashes with existing job", StringComparison.InvariantCultureIgnoreCase));
        }


        #region Private Methods

        // creates a validator which passes everything
        private JobValidator CreateJobValidator(HostRepository? hostRepo = null, JobRepository? jobRepo = null, IDirectoryUtility? dirUtility = null)
        {
            if (hostRepo == null)
            {
                hostRepo = Substitute.For<HostRepository>();
                hostRepo.GetByIdAsync(Arg.Any<IDbContext>(), Arg.Any<int>()).Returns(Task.FromResult(new HostEntity()));
            }

            jobRepo ??= Substitute.For<JobRepository>();

            if (dirUtility == null) 
            { 
                dirUtility = Substitute.For<IDirectoryUtility>();
                dirUtility.Exists(Arg.Any<string>()).Returns(true);
            }


            return new JobValidator(hostRepo, jobRepo, dirUtility);

        }

        private SubstituteBuilder<JobEntity> CreateValidJobEntityBuilder()
        {
            return new SubstituteBuilder<JobEntity>()
                .WithRandomProperties()
                .WithProperty(x => x.HostId, 1)
                .WithProperty(x => x.Type, JobType.Upload)
                .WithProperty(x => x.Schedule, "0 * 0 ? * * *")
                .WithProperty(x => x.ScheduleInWords, "Every minute")
                .WithProperty(x => x.LocalArchivePath, "");

        }

        #endregion

    }

}
