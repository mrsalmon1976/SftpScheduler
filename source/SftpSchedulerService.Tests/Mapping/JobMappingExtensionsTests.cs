using SftpScheduler.BLL.Models;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Mapping;
using SftpSchedulerService.Models.Job;

namespace SftpSchedulerService.Tests.Mapping
{
    [TestFixture]
    public class JobMappingExtensionsTests
    {
        [Test]
        public void ToViewModel_MapsAllProperties()
        {
            var jobEntity = new SubstituteBuilder<JobEntity>().WithRandomProperties().Build();

            JobViewModel result = jobEntity.ToViewModel();

            Assert.That(result.Id, Is.EqualTo(jobEntity.Id));
            Assert.That(result.Name, Is.EqualTo(jobEntity.Name));
            Assert.That(result.HostId, Is.EqualTo(jobEntity.HostId));
            Assert.That(result.Type, Is.EqualTo(jobEntity.Type));
            Assert.That(result.Schedule, Is.EqualTo(jobEntity.Schedule));
            Assert.That(result.ScheduleInWords, Is.EqualTo(jobEntity.ScheduleInWords));
            Assert.That(result.LocalPath, Is.EqualTo(jobEntity.LocalPath));
            Assert.That(result.RemotePath, Is.EqualTo(jobEntity.RemotePath));
            Assert.That(result.DeleteAfterDownload, Is.EqualTo(jobEntity.DeleteAfterDownload));
            Assert.That(result.RemoteArchivePath, Is.EqualTo(jobEntity.RemoteArchivePath));
            Assert.That(result.LocalArchivePath, Is.EqualTo(jobEntity.LocalArchivePath));
            Assert.That(result.LocalPrefix, Is.EqualTo(jobEntity.LocalPrefix));
            Assert.That(result.LocalCopyPaths, Is.EqualTo(jobEntity.LocalCopyPaths));
            Assert.That(result.IsEnabled, Is.EqualTo(jobEntity.IsEnabled));
            Assert.That(result.RestartOnFailure, Is.EqualTo(jobEntity.RestartOnFailure));
            Assert.That(result.CompressionMode, Is.EqualTo(jobEntity.CompressionMode));
            Assert.That(result.FileMask, Is.EqualTo(jobEntity.FileMask));
            Assert.That(result.PreserveTimestamp, Is.EqualTo(jobEntity.PreserveTimestamp));
            Assert.That(result.TransferMode, Is.EqualTo(jobEntity.TransferMode));
        }

        [Test]
        public void ToEntity_MapsAllProperties()
        {
            var jobViewModel = new SubstituteBuilder<JobViewModel>().WithRandomProperties().Build();

            JobEntity result = jobViewModel.ToEntity();

            Assert.That(result.Id, Is.EqualTo(jobViewModel.Id));
            Assert.That(result.Name, Is.EqualTo(jobViewModel.Name));
            Assert.That(result.HostId, Is.EqualTo(jobViewModel.HostId));
            Assert.That(result.Type, Is.EqualTo(jobViewModel.Type));
            Assert.That(result.Schedule, Is.EqualTo(jobViewModel.Schedule));
            Assert.That(result.LocalPath, Is.EqualTo(jobViewModel.LocalPath));
            Assert.That(result.RemotePath, Is.EqualTo(jobViewModel.RemotePath));
            Assert.That(result.DeleteAfterDownload, Is.EqualTo(jobViewModel.DeleteAfterDownload));
            Assert.That(result.RemoteArchivePath, Is.EqualTo(jobViewModel.RemoteArchivePath));
            Assert.That(result.LocalArchivePath, Is.EqualTo(jobViewModel.LocalArchivePath));
            Assert.That(result.LocalPrefix, Is.EqualTo(jobViewModel.LocalPrefix));
            Assert.That(result.LocalCopyPaths, Is.EqualTo(jobViewModel.LocalCopyPaths));
            Assert.That(result.IsEnabled, Is.EqualTo(jobViewModel.IsEnabled));
            Assert.That(result.RestartOnFailure, Is.EqualTo(jobViewModel.RestartOnFailure));
            Assert.That(result.CompressionMode, Is.EqualTo(jobViewModel.CompressionMode));
            Assert.That(result.FileMask, Is.EqualTo(jobViewModel.FileMask));
            Assert.That(result.PreserveTimestamp, Is.EqualTo(jobViewModel.PreserveTimestamp));
            Assert.That(result.TransferMode, Is.EqualTo(jobViewModel.TransferMode));
        }
    }
}
