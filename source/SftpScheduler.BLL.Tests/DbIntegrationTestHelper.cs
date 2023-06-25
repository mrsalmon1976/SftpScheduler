using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Utility;

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
			GlobalUserSettingEntity settingEntity = EntityTestHelper.CreateGlobalUserSettingEntity(id, settingValue);

			string sql = @"INSERT INTO GlobalUserSetting (Id, SettingValue) VALUES (@Id, @SettingValue)";
			dbContext.ExecuteNonQueryAsync(sql, settingEntity).GetAwaiter().GetResult();

			return settingEntity;

		}

		internal HostEntity CreateHostEntity(IDbContext dbContext)
        {
            HostEntity hostEntity = EntityTestHelper.CreateHostEntity();

            string sql = @"INSERT INTO Host (Name, Host, Port, Username, Password, Created) VALUES (@Name, @Host, @Port, @Username, @Password, @Created)";
            dbContext.ExecuteNonQueryAsync(sql, hostEntity).GetAwaiter().GetResult();

            sql = @"select last_insert_rowid()";
            hostEntity.Id = dbContext.ExecuteScalarAsync<int>(sql).GetAwaiter().GetResult();

            return hostEntity;

        }

        internal JobEntity CreateJobEntity(IDbContext dbContext, int hostId, bool isEnabled = true)
        {
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.HostId = hostId;
            jobEntity.IsEnabled = isEnabled;

            string sql = @"INSERT INTO Job (Name, HostId, Type, Schedule, ScheduleInWords, LocalPath, RemotePath, IsEnabled, Created) VALUES (@Name, @HostId, @Type, @Schedule, @ScheduleInWords, @LocalPath, @RemotePath, @IsEnabled, @Created)";
            dbContext.ExecuteNonQueryAsync(sql, jobEntity).GetAwaiter().GetResult();

            sql = @"select last_insert_rowid()";
            jobEntity.Id = dbContext.ExecuteScalarAsync<int>(sql).GetAwaiter().GetResult();

            return jobEntity;

        }

        internal JobLogEntity CreateJobLogEntity(IDbContext dbContext, int jobId, string status = JobStatus.InProgress)
        {
            JobLogEntity jobLog = EntityTestHelper.CreateJobLogEntity();
            jobLog.JobId = jobId;
            jobLog.Status = status;
            jobLog.StartDate = DateTime.Now;

            if (jobLog.Status !=  JobStatus.InProgress)
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
