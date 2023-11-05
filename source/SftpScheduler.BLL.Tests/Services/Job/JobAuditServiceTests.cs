using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Services.Job;
using SftpScheduler.BLL.Tests.Builders.Data;
using SftpScheduler.BLL.Tests.Builders.Models;
using SftpScheduler.BLL.Tests.Builders.Repositories;
using SftpScheduler.BLL.Utility;

namespace SftpScheduler.BLL.Tests.Services.Job
{
    [TestFixture]
    public class JobAuditServiceTests
    {

        [Test]
        public void CompareJobs_NoPropertiesChanged_NoAuditRecordCreated()
        {
            // setup
            IDbContext dbContext = new DbContextBuilder().Build();
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
            IDbContext dbContext = new DbContextBuilder().Build();
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
            IDbContext dbContext = new DbContextBuilder().Build();
            JobEntity jobEntityOld = new JobEntityBuilder().WithRandomProperties().Build();
            JobEntity jobEntityNew = ObjectUtils.Clone<JobEntity>(jobEntityOld)!;
            jobEntityNew.HostId = jobEntityNew.HostId + 1;

            HostRepository hostRepo = new HostRepositoryBuilder().Build();  
            HostEntity hostEntityOld = new HostEntityBuilder().WithId(jobEntityOld.HostId).Build();
            hostRepo.GetByIdAsync(dbContext, hostEntityOld.Id).Returns(Task.FromResult(hostEntityOld));
            HostEntity hostEntityNew = new HostEntityBuilder().WithId(jobEntityNew.HostId).Build();
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
            IDbContext dbContext = new DbContextBuilder().Build();
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
            IDbContext dbContext = new DbContextBuilder().Build();
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
            IDbContext dbContext = new DbContextBuilder().Build();
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
            IDbContext dbContext = new DbContextBuilder().Build();
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
            IDbContext dbContext = new DbContextBuilder().Build();
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
            IDbContext dbContext = new DbContextBuilder().Build();
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
            IDbContext dbContext = new DbContextBuilder().Build();
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
            IDbContext dbContext = new DbContextBuilder().Build();
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
        public void CompareJobs_AllPropertiesChanged_AuditRecordsCreated()
        {
            // setup
            IDbContext dbContext = new DbContextBuilder().Build();

            HostEntity hostEntityOld = new HostEntityBuilder().WithRandomProperties().Build();
            HostEntity hostEntityNew = new HostEntityBuilder().WithRandomProperties().Build();

            HostRepository hostRepo = new HostRepositoryBuilder().Build();
            hostRepo.GetByIdAsync(dbContext, hostEntityOld.Id).Returns(Task.FromResult(hostEntityOld)!);
            hostRepo.GetByIdAsync(dbContext, hostEntityNew.Id).Returns(Task.FromResult(hostEntityNew)!);


            string userName = Guid.NewGuid().ToString();
            JobEntity jobEntityOld = new JobEntityBuilder().WithRandomProperties()
                .WithHostId(hostEntityOld.Id)
                .WithType(JobType.Download)
                .WithDeleteAfterDownload(true)
                .WithLocalCopyPaths(Guid.NewGuid().ToString())
                .WithIsEnabled(true)
                .Build();
            JobEntity jobEntityNew = new JobEntityBuilder()
                .WithName(Guid.NewGuid().ToString())
                .WithHostId(hostEntityNew.Id)
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
            IJobAuditService jobAuditService = new JobAuditService(hostRepo);
            var result = jobAuditService.CompareJobs(dbContext, jobEntityOld, jobEntityNew, userName).Result;

            // assert
            Assert.That(result.Count(), Is.EqualTo(10));
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
        }
    

        [Test]
        public void CompareJobs_PropertiesChanged_UserNameCorrectlySet()
        {
            // setup
            IDbContext dbContext = new DbContextBuilder().Build();
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
