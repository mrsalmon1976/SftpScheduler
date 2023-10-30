using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Tests.Builders.Models;
using SftpScheduler.BLL.Utility;
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
			GlobalUserSettingEntity settingEntity = new GlobalUserSettingEntityBuilder()
                .WithId(id)
                .WithSettingValue(settingValue)
                .Build();

			string sql = @"INSERT INTO GlobalUserSetting (Id, SettingValue) VALUES (@Id, @SettingValue)";
			dbContext.ExecuteNonQueryAsync(sql, settingEntity).GetAwaiter().GetResult();

			return settingEntity;

		}

		internal HostEntity CreateHostEntity(IDbContext dbContext)
        {
            HostEntity hostEntity = new HostEntityBuilder().WithRandomProperties().WithId(0).Build();

            string sql = @"INSERT INTO Host (Name, Host, Port, Username, Password, Created) VALUES (@Name, @Host, @Port, @Username, @Password, @Created)";
            dbContext.ExecuteNonQueryAsync(sql, hostEntity).GetAwaiter().GetResult();

            sql = @"select last_insert_rowid()";
            hostEntity.Id = dbContext.ExecuteScalarAsync<int>(sql).GetAwaiter().GetResult();

            return hostEntity;

        }

        internal HostAuditLogEntity CreateHostAuditLogEntity(IDbContext dbContext, int hostId)
        {
            HostAuditLogEntity hostAuditLogEntity = new HostAuditLogEntityBuilder().WithRandomProperties().WithHostId(hostId).Build();

            string sql = @"INSERT INTO HostAuditLog (HostId, PropertyName, FromValue, ToValue, UserName, Created) VALUES (@HostId, @PropertyName, @FromValue, @ToValue, @UserName, @Created)";
            dbContext.ExecuteNonQueryAsync(sql, hostAuditLogEntity).GetAwaiter().GetResult();

            sql = @"select last_insert_rowid()";
            hostAuditLogEntity.Id = dbContext.ExecuteScalarAsync<int>(sql).GetAwaiter().GetResult();

            return hostAuditLogEntity;

        }

        internal JobEntity CreateJobEntity(IDbContext dbContext, int hostId, bool isEnabled = true)
        {
            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties()
                .WithHostId(hostId)
                .WithIsEnabled(isEnabled)
                .Build();

            string sql = @"INSERT INTO Job (Name, HostId, Type, Schedule, ScheduleInWords, LocalPath, RemotePath, IsEnabled, Created) VALUES (@Name, @HostId, @Type, @Schedule, @ScheduleInWords, @LocalPath, @RemotePath, @IsEnabled, @Created)";
            dbContext.ExecuteNonQueryAsync(sql, jobEntity).GetAwaiter().GetResult();

            sql = @"select last_insert_rowid()";
            jobEntity.Id = dbContext.ExecuteScalarAsync<int>(sql).GetAwaiter().GetResult();

            return jobEntity;

        }

        internal JobFileLogEntity CreateJobFileLogEntity(IDbContext dbContext, int jobId)
        {
            JobFileLogEntity jobFileLog = new JobFileLogEntityBuilder().WithRandomProperties()
                .WithJobId(jobId)
                .WithStartDate(DateTime.Now)
                .Build();

            string sql = @"INSERT INTO JobFileLog (JobId, FileName, FileLength, StartDate, EndDate) VALUES (@JobId, @FileName, @FileLength, @StartDate, @EndDate)";
            dbContext.ExecuteNonQueryAsync(sql, jobFileLog).GetAwaiter().GetResult();

            sql = @"select last_insert_rowid()";
            jobFileLog.Id = dbContext.ExecuteScalarAsync<int>(sql).GetAwaiter().GetResult();

            return jobFileLog;

        }

        internal JobLogEntity CreateJobLogEntity(IDbContext dbContext, int jobId, string status = JobStatus.InProgress)
        {
            JobLogEntity jobLog = new JobLogEntityBuilder().WithRandomProperties()
                .WithJobId(jobId)
                .WithStatus(status)
                .WithStartDate(DateTime.Now)
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
