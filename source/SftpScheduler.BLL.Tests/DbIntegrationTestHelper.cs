using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Utility;
using SftpScheduler.Test.Common;
using static Quartz.Logging.OperationName;

namespace SftpScheduler.BLL.Tests
{
    internal class DbIntegrationTestHelper : IDisposable
    {
        public DbIntegrationTestHelper()
        {
            this.DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DbIntegrationTest.db");
            this.DbContextFactory = new DbContextFactory(this.DbPath);
            if (File.Exists(this.DbPath))
            {
                File.Delete(this.DbPath);
            }
        }

        public void Dispose()
        {
            if (File.Exists(this.DbPath))
            {
                File.Delete(this.DbPath);
            }
        }

        internal IDbContextFactory DbContextFactory { get; set; }

        internal string? DbPath { get; set; }

        internal virtual void CreateDatabase()
        {
            if (String.IsNullOrWhiteSpace(DbPath))
            {
                throw new InvalidOperationException("DbPath has not been set");
            }

            IDbMigrator dbMigrator = new SQLiteDbMigrator(this.DbContextFactory, new ResourceUtils());
            dbMigrator.MigrateDbChanges();
        }

		internal GlobalUserSettingEntity CreateGlobalUserSettingEntity(IDbContext dbContext, string id, string settingValue)
		{
			GlobalUserSettingEntity settingEntity = new SubstituteBuilder<GlobalUserSettingEntity>()
                .WithProperty(x => x.Id, id)
                .WithProperty(x => x.SettingValue, settingValue)
                .Build();

			string sql = @"INSERT INTO GlobalUserSetting (Id, SettingValue) VALUES (@Id, @SettingValue)";
			dbContext.ExecuteNonQueryAsync(sql, settingEntity).GetAwaiter().GetResult();

			return settingEntity;

		}

		internal HostEntity CreateHostEntity(IDbContext dbContext)
        {
            HostEntity hostEntity = new SubstituteBuilder<HostEntity>()
                .WithRandomProperties()
                .WithProperty(x => x.Id, 0)
                .WithProperty(x => x.Host, RandomData.IPAddress().ToString())
                .WithProperty(x => x.Port, RandomData.Number(1, 65000))
                .WithProperty(x => x.KeyFingerprint, String.Empty)
                .WithProperty(x => x.Created, DateTime.UtcNow)
                .Build();

            string sql = @"INSERT INTO Host (Name, Host, Port, Username, Password, KeyFingerprint, Created, Protocol, FtpsMode) VALUES (@Name, @Host, @Port, @Username, @Password, @KeyFingerprint, @Created, @Protocol, @FtpsMode)";
            dbContext.ExecuteNonQueryAsync(sql, hostEntity).GetAwaiter().GetResult();

            sql = @"select last_insert_rowid()";
            hostEntity.Id = dbContext.ExecuteScalarAsync<int>(sql).GetAwaiter().GetResult();

            return hostEntity;

        }

        internal HostAuditLogEntity CreateHostAuditLogEntity(IDbContext dbContext, int hostId)
        {
            HostAuditLogEntity hostAuditLogEntity = new SubstituteBuilder<HostAuditLogEntity>().WithRandomProperties().WithProperty(x => x.HostId, hostId).Build();

            string sql = @"INSERT INTO HostAuditLog (HostId, PropertyName, FromValue, ToValue, UserName, Created) VALUES (@HostId, @PropertyName, @FromValue, @ToValue, @UserName, @Created)";
            dbContext.ExecuteNonQueryAsync(sql, hostAuditLogEntity).GetAwaiter().GetResult();

            sql = @"select last_insert_rowid()";
            hostAuditLogEntity.Id = dbContext.ExecuteScalarAsync<int>(sql).GetAwaiter().GetResult();

            return hostAuditLogEntity;

        }

        internal JobEntity CreateJobEntity(IDbContext dbContext, int hostId, bool isEnabled = true)
        {
            JobEntity jobEntity = new SubstituteBuilder<JobEntity>().WithRandomProperties()
                .WithProperty(x => x.HostId, hostId)
                .WithProperty(x => x.IsEnabled, isEnabled)
                .Build();

            string sql = @"INSERT INTO Job 
                (Name, HostId, Type, Schedule, ScheduleInWords, LocalPath, RemotePath, DeleteAfterDownload, RemoteArchivePath, LocalCopyPaths, IsEnabled, Created, RestartOnFailure, CompressionMode, FileMask, PreserveTimestamp, TransferMode) 
                VALUES 
                (@Name, @HostId, @Type, @Schedule, @ScheduleInWords, @LocalPath, @RemotePath, @DeleteAfterDownload, @RemoteArchivePath, @LocalCopyPaths, @IsEnabled, @Created, @RestartOnFailure, @CompressionMode, @FileMask, @PreserveTimestamp, @TransferMode)";
            dbContext.ExecuteNonQueryAsync(sql, jobEntity).GetAwaiter().GetResult();

            sql = @"select last_insert_rowid()";
            jobEntity.Id = dbContext.ExecuteScalarAsync<int>(sql).GetAwaiter().GetResult();

            return jobEntity;

        }

        internal JobAuditLogEntity CreateJobAuditLogEntity(IDbContext dbContext, int jobId)
        {
            JobAuditLogEntity jobAuditLogEntity = new SubstituteBuilder<JobAuditLogEntity>().WithRandomProperties().WithProperty(x => x.JobId, jobId).Build();

            string sql = @"INSERT INTO JobAuditLog (JobId, PropertyName, FromValue, ToValue, UserName, Created) VALUES (@JobId, @PropertyName, @FromValue, @ToValue, @UserName, @Created)";
            dbContext.ExecuteNonQueryAsync(sql, jobAuditLogEntity).GetAwaiter().GetResult();

            sql = @"select last_insert_rowid()";
            jobAuditLogEntity.Id = dbContext.ExecuteScalarAsync<int>(sql).GetAwaiter().GetResult();

            return jobAuditLogEntity;

        }

        internal JobFileLogEntity CreateJobFileLogEntity(IDbContext dbContext, int jobId, DateTime? startDate = null)
        {
            startDate ??= DateTime.Now;

            JobFileLogEntity jobFileLog = new SubstituteBuilder<JobFileLogEntity>().WithRandomProperties()
                .WithProperty(x => x.JobId, jobId)
                .WithProperty(x => x.StartDate, startDate)
                .Build();

            string sql = @"INSERT INTO JobFileLog (JobId, FileName, FileLength, StartDate, EndDate) VALUES (@JobId, @FileName, @FileLength, @StartDate, @EndDate)";
            dbContext.ExecuteNonQueryAsync(sql, jobFileLog).GetAwaiter().GetResult();

            sql = @"select last_insert_rowid()";
            jobFileLog.Id = dbContext.ExecuteScalarAsync<int>(sql).GetAwaiter().GetResult();

            return jobFileLog;

        }

        internal JobLogEntity CreateJobLogEntity(IDbContext dbContext, int jobId, string status = JobStatus.InProgress)
        {
            JobLogEntity jobLog = new SubstituteBuilder<JobLogEntity>().WithRandomProperties()
                .WithProperty(x => x.JobId, jobId)
                .WithProperty(x => x.Status, status)
                .WithProperty(x => x.StartDate, DateTime.Now)
                .Build();

            if (jobLog.Status != JobStatus.InProgress)
            {
                jobLog.EndDate = jobLog.StartDate.AddSeconds(5);
            }

            string sql = @"INSERT INTO JobLog (JobId, StartDate, EndDate, Progress, Status) VALUES (@JobId, @StartDate, @EndDate, @Progress, @Status)";
            dbContext.ExecuteNonQueryAsync(sql, jobLog).GetAwaiter().GetResult();

            sql = @"select last_insert_rowid()";
            jobLog.Id = dbContext.ExecuteScalarAsync<int>(sql).GetAwaiter().GetResult();

            return jobLog;

        }
    }
}
