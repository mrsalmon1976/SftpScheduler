using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SftpScheduler.BLL.Command.Job;
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
            dbContext.ExecuteNonQuery(sql, hostEntity);

            sql = @"select last_insert_rowid()";
            hostEntity.Id = dbContext.ExecuteScalar<int>(sql);

            return hostEntity;

        }

        internal JobEntity CreateJobEntity(IDbContext dbContext, int hostId)
        {
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity(hostId);

            string sql = @"INSERT INTO Job (Name, HostId, Created) VALUES (@Name, @HostId, @Created)";
            dbContext.ExecuteNonQuery(sql, jobEntity);

            sql = @"select last_insert_rowid()";
            jobEntity.Id = dbContext.ExecuteScalar<int>(sql);

            return jobEntity;

        }

    }
}
