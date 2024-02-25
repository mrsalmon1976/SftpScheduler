using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Services.Job;
using SftpScheduler.BLL.Tests.Builders.Models;
using SftpScheduler.BLL.Utility;
using SftpScheduler.Test.Common;

namespace SftpScheduler.BLL.Tests.Services.Job
{
    [TestFixture]
    public class JobAuditServiceTests
    {

        [Test]
        public void CompareJobs_NoPropertiesChanged_NoAuditRecordCreated()
        {
            // setup
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            JobEntity jobEntityOld = new JobEntityBuilder().WithRandomProperties().Build();
            JobEntity hostEntityNew = ObjectUtils.Clone<JobEntity>(jobEntityOld)!;
            string userName = Guid.NewGuid().ToString();

            // execute 
            IJobAuditService jobAuditService = new JobAuditService(new HostRepository());
            var result = jobAuditService.CompareJobs(dbContext, jobEntityOld, hostEntityNew, userName).Result;

            // assert
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public void CompareJobs_NameChanged_AuditRecordCreated()
        {
            // setup
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            JobEntity jobEntityOld = new JobEntityBuilder().WithRandomProperties().Build();
            JobEntity hostEntityNew = ObjectUtils.Clone<JobEntity>(jobEntityOld)!;
            hostEntityNew.Name = Guid.NewGuid().ToString();
            string userName = Guid.NewGuid().ToString();

            // execute 
            IJobAuditService jobAuditService = new JobAuditService(new HostRepository());
            var result = jobAuditService.CompareJobs(dbContext, jobEntityOld, hostEntityNew, userName).Result;

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), jobEntityOld.Id, "Name", jobEntityOld.Name, hostEntityNew.Name);
        }

        [Test]
        public void CompareJobs_HostChanged_AuditRecordCreated()
        {
            // setup
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            JobEntity jobEntityOld = new JobEntityBuilder().WithRandomProperties().Build();
            JobEntity jobEntityNew = ObjectUtils.Clone<JobEntity>(jobEntityOld)!;
            jobEntityNew.HostId = jobEntityNew.HostId + 1;

            HostRepository hostRepo = new SubstituteBuilder<HostRepository>().Build();  
            HostEntity hostEntityOld = new SubstituteBuilder<HostEntity>().WithRandomProperties().WithProperty(x => x.Id, jobEntityOld.HostId).Build();
            hostRepo.GetByIdAsync(dbContext, hostEntityOld.Id).Returns(Task.FromResult(hostEntityOld));
            HostEntity hostEntityNew = new SubstituteBuilder<HostEntity>().WithRandomProperties().WithProperty(x => x.Id, jobEntityNew.HostId).Build();
            hostRepo.GetByIdAsync(dbContext, hostEntityNew.Id).Returns(Task.FromResult(hostEntityNew));
            string userName = Guid.NewGuid().ToString();

            // execute 
            IJobAuditService jobAuditService = new JobAuditService(hostRepo);
            var result = jobAuditService.CompareJobs(dbContext, jobEntityOld, jobEntityNew, userName).Result;

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), jobEntityOld.Id, "Host", $"{hostEntityOld.Name}", $"{hostEntityNew.Name}");
        }

        [Test]
        public void CompareJobs_TypeChanged_AuditRecordCreated()
        {
            // setup
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            JobEntity jobEntityOld = new JobEntityBuilder().WithRandomProperties().WithType(JobType.Download).Build();
            JobEntity jobEntityNew = ObjectUtils.Clone<JobEntity>(jobEntityOld)!;
            jobEntityNew.Type = JobType.Upload;
            string userName = Guid.NewGuid().ToString();

            // execute 
            IJobAuditService jobAuditService = new JobAuditService(new HostRepository());
            var result = jobAuditService.CompareJobs(dbContext, jobEntityOld, jobEntityNew, userName).Result;

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), jobEntityOld.Id, "JobType", JobType.Download.ToString(), JobType.Upload.ToString());
        }

        [Test]
        public void CompareJobs_ScheduleChanged_AuditRecordCreated()
        {
            // setup
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            JobEntity jobEntityOld = new JobEntityBuilder().WithRandomProperties().Build();
            JobEntity jobEntityNew = ObjectUtils.Clone<JobEntity>(jobEntityOld)!;
            jobEntityNew.Schedule = Guid.NewGuid().ToString();
            string userName = Guid.NewGuid().ToString();

            // execute 
            IJobAuditService jobAuditService = new JobAuditService(new HostRepository());
            var result = jobAuditService.CompareJobs(dbContext, jobEntityOld, jobEntityNew, userName).Result;

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), jobEntityOld.Id, "Schedule", jobEntityOld.Schedule, jobEntityNew.Schedule);
        }

        [Test]
        public void CompareJobs_LocalPathChanged_AuditRecordCreated()
        {
            // setup
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            JobEntity jobEntityOld = new JobEntityBuilder().WithRandomProperties().Build();
            JobEntity jobEntityNew = ObjectUtils.Clone<JobEntity>(jobEntityOld)!;
            jobEntityNew.LocalPath = Guid.NewGuid().ToString();
            string userName = Guid.NewGuid().ToString();

            // execute 
            IJobAuditService jobAuditService = new JobAuditService(new HostRepository());
            var result = jobAuditService.CompareJobs(dbContext, jobEntityOld, jobEntityNew, userName).Result;

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), jobEntityOld.Id, "LocalPath", jobEntityOld.LocalPath, jobEntityNew.LocalPath);
        }

        [Test]
        public void CompareJobs_RemotePathChanged_AuditRecordCreated()
        {
            // setup
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            JobEntity jobEntityOld = new JobEntityBuilder().WithRandomProperties().Build();
            JobEntity jobEntityNew = ObjectUtils.Clone<JobEntity>(jobEntityOld)!;
            jobEntityNew.RemotePath = Guid.NewGuid().ToString();
            string userName = Guid.NewGuid().ToString();

            // execute 
            IJobAuditService jobAuditService = new JobAuditService(new HostRepository());
            var result = jobAuditService.CompareJobs(dbContext, jobEntityOld, jobEntityNew, userName).Result;

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), jobEntityOld.Id, "RemotePath", jobEntityOld.RemotePath, jobEntityNew.RemotePath);
        }

        [Test]
        public void CompareJobs_DeleteAfterDownloadChanged_AuditRecordCreated()
        {
            // setup
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            JobEntity jobEntityOld = new JobEntityBuilder().WithRandomProperties().WithDeleteAfterDownload(true).Build();
            JobEntity jobEntityNew = ObjectUtils.Clone<JobEntity>(jobEntityOld)!;
            jobEntityNew.DeleteAfterDownload = false;
            string userName = Guid.NewGuid().ToString();

            // execute 
            IJobAuditService jobAuditService = new JobAuditService(new HostRepository());
            var result = jobAuditService.CompareJobs(dbContext, jobEntityOld, jobEntityNew, userName).Result;

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), jobEntityOld.Id, "DeleteAfterDownload", "True", "False");
        }


        [Test]
        public void CompareJobs_RemoteArchivePathChanged_AuditRecordCreated()
        {
            // setup
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            JobEntity jobEntityOld = new JobEntityBuilder().WithRandomProperties().Build();
            JobEntity jobEntityNew = ObjectUtils.Clone<JobEntity>(jobEntityOld)!;
            jobEntityNew.RemoteArchivePath = Guid.NewGuid().ToString();
            string userName = Guid.NewGuid().ToString();

            // execute 
            IJobAuditService jobAuditService = new JobAuditService(new HostRepository());
            var result = jobAuditService.CompareJobs(dbContext, jobEntityOld, jobEntityNew, userName).Result;

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), jobEntityOld.Id, "RemoteArchivePath", jobEntityOld.RemoteArchivePath!, jobEntityNew.RemoteArchivePath);
        }

        [Test]
        public void CompareJobs_LocalCopyPathsChanged_AuditRecordCreated()
        {
            // setup
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            JobEntity jobEntityOld = new JobEntityBuilder()
                .WithRandomProperties()
                .WithLocalCopyPaths(Guid.NewGuid().ToString())
                .Build();
            JobEntity jobEntityNew = ObjectUtils.Clone<JobEntity>(jobEntityOld)!;
            jobEntityNew.LocalCopyPaths = Guid.NewGuid().ToString();
            string userName = Guid.NewGuid().ToString();

            // execute 
            IJobAuditService jobAuditService = new JobAuditService(new HostRepository());
            var result = jobAuditService.CompareJobs(dbContext, jobEntityOld, jobEntityNew, userName).Result;

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), jobEntityOld.Id, "LocalCopyPaths", jobEntityOld.LocalCopyPaths!, jobEntityNew.LocalCopyPaths);
        }

        [Test]
        public void CompareJobs_IsEnabledChanged_AuditRecordCreated()
        {
            // setup
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            JobEntity jobEntityOld = new JobEntityBuilder().WithRandomProperties().WithIsEnabled(true).Build();
            JobEntity jobEntityNew = ObjectUtils.Clone<JobEntity>(jobEntityOld)!;
            jobEntityNew.IsEnabled = false;
            string userName = Guid.NewGuid().ToString();

            // execute 
            IJobAuditService jobAuditService = new JobAuditService(new HostRepository());
            var result = jobAuditService.CompareJobs(dbContext, jobEntityOld, jobEntityNew, userName).Result;

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), jobEntityOld.Id, "Enabled", "True", "False");
        }

        [Test]
        public void CompareJobs_RestartOnFailureChanged_AuditRecordCreated()
        {
            // setup
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            JobEntity jobEntityOld = new JobEntityBuilder().WithRandomProperties().WithRestartOnFailure(true).Build();
            JobEntity jobEntityNew = ObjectUtils.Clone<JobEntity>(jobEntityOld)!;
            jobEntityNew.RestartOnFailure = false;
            string userName = Guid.NewGuid().ToString();

            // execute 
            IJobAuditService jobAuditService = new JobAuditService(new HostRepository());
            var result = jobAuditService.CompareJobs(dbContext, jobEntityOld, jobEntityNew, userName).Result;

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), jobEntityOld.Id, "RestartOnFailure", "True", "False");
        }

        [Test]
        public void CompareJobs_CompressionModeChanged_AuditRecordCreated()
        {
            // setup
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            JobEntity jobEntityOld = new SubstituteBuilder<JobEntity>()
                .WithRandomProperties()
                .WithProperty(x => x.CompressionMode, CompressionMode.None).Build();
            JobEntity jobEntityNew = ObjectUtils.Clone<JobEntity>(jobEntityOld)!;
            jobEntityNew.CompressionMode = CompressionMode.Zip;
            string userName = Guid.NewGuid().ToString();

            // execute 
            IJobAuditService jobAuditService = new JobAuditService(new HostRepository());
            var result = jobAuditService.CompareJobs(dbContext, jobEntityOld, jobEntityNew, userName).Result;

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), jobEntityOld.Id, "CompressionMode", "None", "Zip");
        }

        [Test]
        public void CompareJobs_FileMaskChanged_AuditRecordCreated()
        {
            // setup
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            JobEntity jobEntityOld = new SubstituteBuilder<JobEntity>()
                .WithRandomProperties()
                .WithProperty(x => x.FileMask, "*.old").Build();
            JobEntity jobEntityNew = ObjectUtils.Clone<JobEntity>(jobEntityOld)!;
            jobEntityNew.FileMask = "*.new";
            string userName = Guid.NewGuid().ToString();

            // execute 
            IJobAuditService jobAuditService = new JobAuditService(new HostRepository());
            var result = jobAuditService.CompareJobs(dbContext, jobEntityOld, jobEntityNew, userName).Result;

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), jobEntityOld.Id, "FileMask", "*.old", "*.new");
        }

        [Test]
        public void CompareJobs_PreserveTimestampChanged_AuditRecordCreated()
        {
            // setup
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            JobEntity jobEntityOld = new SubstituteBuilder<JobEntity>()
                .WithRandomProperties()
                .WithProperty(x => x.PreserveTimestamp, true).Build();
            JobEntity jobEntityNew = ObjectUtils.Clone<JobEntity>(jobEntityOld)!;
            jobEntityNew.PreserveTimestamp = false;
            string userName = Guid.NewGuid().ToString();

            // execute 
            IJobAuditService jobAuditService = new JobAuditService(new HostRepository());
            var result = jobAuditService.CompareJobs(dbContext, jobEntityOld, jobEntityNew, userName).Result;

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), jobEntityOld.Id, "PreserveTimestamp", "True", "False");
        }


        [Test]
        public void CompareJobs_TransferModeChanged_AuditRecordCreated()
        {
            // setup
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            JobEntity jobEntityOld = new SubstituteBuilder<JobEntity>()
                .WithRandomProperties()
                .WithProperty(x => x.TransferMode, TransferMode.Binary).Build();
            JobEntity jobEntityNew = ObjectUtils.Clone<JobEntity>(jobEntityOld)!;
            jobEntityNew.TransferMode = TransferMode.Automatic;
            string userName = Guid.NewGuid().ToString();

            // execute 
            IJobAuditService jobAuditService = new JobAuditService(new HostRepository());
            var result = jobAuditService.CompareJobs(dbContext, jobEntityOld, jobEntityNew, userName).Result;

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), jobEntityOld.Id, "TransferMode", "Binary", "Automatic");
        }

        [Test]
        public void CompareJobs_AllPropertiesChanged_AuditRecordsCreated()
        {
            // setup
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();

            HostEntity hostEntityOld = new SubstituteBuilder<HostEntity>().WithRandomProperties().Build();
            HostEntity hostEntityNew = new SubstituteBuilder<HostEntity>().WithRandomProperties().Build();

            HostRepository hostRepo = new SubstituteBuilder<HostRepository>().Build();
            hostRepo.GetByIdAsync(dbContext, hostEntityOld.Id).Returns(Task.FromResult(hostEntityOld)!);
            hostRepo.GetByIdAsync(dbContext, hostEntityNew.Id).Returns(Task.FromResult(hostEntityNew)!);


            string userName = Guid.NewGuid().ToString();
            JobEntity jobEntityOld = new SubstituteBuilder<JobEntity>()
                .WithProperty(x => x.Name, Guid.NewGuid().ToString())
                .WithProperty(x => x.HostId, hostEntityOld.Id)
                .WithProperty(x => x.Type, JobType.Download)
                .WithProperty(x => x.Schedule, Guid.NewGuid().ToString())
                .WithProperty(x => x.LocalPath, Guid.NewGuid().ToString())
                .WithProperty(x => x.RemotePath, Guid.NewGuid().ToString())
                .WithProperty(x => x.DeleteAfterDownload, true)
                .WithProperty(x => x.RemoteArchivePath, Guid.NewGuid().ToString())
                .WithProperty(x => x.LocalCopyPaths, Guid.NewGuid().ToString())
                .WithProperty(x => x.IsEnabled, true)
                .WithProperty(x => x.RestartOnFailure, true)
                .WithProperty(x => x.CompressionMode, CompressionMode.Zip)
                .WithProperty(x => x.FileMask, "*.OLD")
                .WithProperty(x => x.PreserveTimestamp, false)
                .WithProperty(x => x.TransferMode, TransferMode.Ascii)
                .Build();
            JobEntity jobEntityNew = new SubstituteBuilder<JobEntity>()
                .WithProperty(x => x.Name, Guid.NewGuid().ToString())
                .WithProperty(x => x.HostId, hostEntityNew.Id)
                .WithProperty(x => x.Type, JobType.Upload)
                .WithProperty(x => x.Schedule, Guid.NewGuid().ToString())
                .WithProperty(x => x.LocalPath, Guid.NewGuid().ToString())
                .WithProperty(x => x.RemotePath, Guid.NewGuid().ToString())
                .WithProperty(x => x.DeleteAfterDownload, false)
                .WithProperty(x => x.RemoteArchivePath, Guid.NewGuid().ToString())
                .WithProperty(x => x.LocalCopyPaths, Guid.NewGuid().ToString())
                .WithProperty(x => x.IsEnabled, false)
                .WithProperty(x => x.RestartOnFailure, false)
                .WithProperty(x => x.CompressionMode, CompressionMode.None)
                .WithProperty(x => x.FileMask, "*.NEW")
                .WithProperty(x => x.PreserveTimestamp, true)
                .WithProperty(x => x.TransferMode, TransferMode.Automatic)
                .Build();

            // execute 
            IJobAuditService jobAuditService = new JobAuditService(hostRepo);
            var result = jobAuditService.CompareJobs(dbContext, jobEntityOld, jobEntityNew, userName).Result;

            // assert
            Assert.That(result.Count(), Is.EqualTo(15));
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "Name"), jobEntityOld.Id, "Name", jobEntityOld.Name, jobEntityNew.Name);
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "Host"), jobEntityOld.Id, "Host", $"{hostEntityOld.Name}", $"{hostEntityNew.Name}");
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "JobType"), jobEntityOld.Id, "JobType", jobEntityOld.Type.ToString(), jobEntityNew.Type.ToString());
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "Schedule"), jobEntityOld.Id, "Schedule", jobEntityOld.Schedule!, jobEntityNew.Schedule!);
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "LocalPath"), jobEntityOld.Id, "LocalPath", jobEntityOld.LocalPath!, jobEntityNew.LocalPath!);
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "RemotePath"), jobEntityOld.Id, "RemotePath", jobEntityOld.RemotePath!, jobEntityNew.RemotePath!);
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "DeleteAfterDownload"), jobEntityOld.Id, "DeleteAfterDownload", "True", "False");
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "RemoteArchivePath"), jobEntityOld.Id, "RemoteArchivePath", jobEntityOld.RemoteArchivePath!, jobEntityNew.RemoteArchivePath!);
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "LocalCopyPaths"), jobEntityOld.Id, "LocalCopyPaths", jobEntityOld.LocalCopyPaths!, jobEntityNew.LocalCopyPaths!);
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "Enabled"), jobEntityOld.Id, "Enabled", "True", "False");
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "RestartOnFailure"), jobEntityOld.Id, "RestartOnFailure", "True", "False");
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "CompressionMode"), jobEntityOld.Id, "CompressionMode", jobEntityOld.CompressionMode.ToString(), jobEntityNew.CompressionMode.ToString());
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "FileMask"), jobEntityOld.Id, "FileMask", jobEntityOld.FileMask!, jobEntityNew.FileMask!);
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "PreserveTimestamp"), jobEntityOld.Id, "PreserveTimestamp", "False", "True");
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "TransferMode"), jobEntityOld.Id, "TransferMode", jobEntityOld.TransferMode.ToString(), jobEntityNew.TransferMode.ToString());
        }


        [Test]
        public void CompareJobs_PropertiesChanged_UserNameCorrectlySet()
        {
            // setup
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            string userName = Guid.NewGuid().ToString();
            JobEntity jobEntityOld = new JobEntityBuilder().WithRandomProperties()
                .WithHostId(1)
                .WithType(JobType.Download)
                .WithDeleteAfterDownload(true)
                .WithIsEnabled(true)
                .Build();
            JobEntity jobEntityNew = new JobEntityBuilder()
                .WithName(Guid.NewGuid().ToString())
                .WithHostId(2)
                .WithType(JobType.Upload)
                .WithSchedule(Guid.NewGuid().ToString())
                .WithLocalPath(Guid.NewGuid().ToString())
                .WithRemotePath(Guid.NewGuid().ToString())
                .WithDeleteAfterDownload(false)
                .WithRemoteArchivePath(Guid.NewGuid().ToString())
                .WithLocalCopyPaths(Guid.NewGuid().ToString())
                .WithIsEnabled(false)
                .Build();

            // execute 
            IJobAuditService jobAuditService = new JobAuditService(new HostRepository());
            var auditLogEntities = jobAuditService.CompareJobs(dbContext, jobEntityOld, jobEntityNew, userName).Result;

            // assert
            Assert.That(auditLogEntities.Count(), Is.GreaterThan(1));
            foreach (var auditLogEntity in auditLogEntities)
            {
                Assert.That(auditLogEntity.UserName, Is.EqualTo(userName));
            }
        }

        private void AssertAuditLogProperties(JobAuditLogEntity jobAuditLogEntity, int jobId, string propertyName, string fromValue, string toValue)
        {
            Assert.That(jobAuditLogEntity.JobId, Is.EqualTo(jobId));
            Assert.That(jobAuditLogEntity.PropertyName, Is.EqualTo(propertyName));
            Assert.That(jobAuditLogEntity.FromValue, Is.EqualTo(fromValue));
            Assert.That(jobAuditLogEntity.ToValue, Is.EqualTo(toValue));
        }

    }
}
