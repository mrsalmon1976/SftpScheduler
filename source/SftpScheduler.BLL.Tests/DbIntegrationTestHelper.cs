using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        internal HostEntity CreateHostEntity(IDbContext dbContext)
        {
            HostEntity hostEntity = EntityTestHelper.CreateHostEntity();

            string sql = @"INSERT INTO Host (Name, Host, Port, Username, Password, Created) VALUES (@Name, @Host, @Port, @Username, @Password, @Created)";
            dbContext.ExecuteNonQueryAsync(sql, hostEntity).GetAwaiter().GetResult();

            sql = @"select last_insert_rowid()";
            hostEntity.Id = dbContext.ExecuteScalarAsync<int>(sql).GetAwaiter().GetResult();

            return hostEntity;

        }

        internal JobEntity CreateJobEntity(IDbContext dbContext, int hostId)
        {
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.HostId = hostId;

            string sql = @"INSERT INTO Job (Name, HostId, Type, Schedule, LocalPath, RemotePath, Created) VALUES (@Name, @HostId, @Type, @Schedule, @LocalPath, @RemotePath, @Created)";
            dbContext.ExecuteNonQueryAsync(sql, jobEntity).GetAwaiter().GetResult();

            sql = @"select last_insert_rowid()";
            jobEntity.Id = dbContext.ExecuteScalarAsync<int>(sql).GetAwaiter().GetResult();

            return jobEntity;

        }

        internal JobResultEntity CreateJobResultEntity(IDbContext dbContext, int jobId)
        {
            JobResultEntity jobResultEntity = EntityTestHelper.CreateJobResultEntity();
            jobResultEntity.JobId = jobId;
            jobResultEntity.Status = JobResult.InProgress;

            string sql = @"INSERT INTO JobResult (JobId, StartDate, Progress, Status) VALUES (@JobId, DATETIME(), @Progress, @Status)";
            dbContext.ExecuteNonQueryAsync(sql, jobResultEntity).GetAwaiter().GetResult();

            sql = @"select last_insert_rowid()";
            jobResultEntity.Id = dbContext.ExecuteScalarAsync<int>(sql).GetAwaiter().GetResult();

            return jobResultEntity;

        }

    }
}
