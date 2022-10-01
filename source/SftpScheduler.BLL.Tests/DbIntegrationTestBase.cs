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
    public class DbIntegrationTestBase
    {
        [SetUp]
        public void DbIntegrationTestBase_SetUp()
        {
            this.DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DbIntegrationTest.db");
            this.DbContextFactory = new DbContextFactory(this.DbPath);
            if (File.Exists(this.DbPath))
            {
                File.Delete(this.DbPath);
            }
        }

        [TearDown]
        public void SQLiteDbMigratorTests_TearDown()
        {
            if (File.Exists(this.DbPath))
            {
                File.Delete(this.DbPath);
            }
        }

        protected IDbContextFactory DbContextFactory { get; set; }

        protected string? DbPath { get; set; }

        public virtual void CreateDatabase()
        {
            if (String.IsNullOrWhiteSpace(DbPath))
            {
                throw new InvalidOperationException("DbPath has not been set");
            }

            IDbMigrator dbMigrator = new SQLiteDbMigrator(this.DbContextFactory, new ResourceUtils());
            dbMigrator.MigrateDbChanges();
        }

        protected JobEntity CreateJobEntity(IDbContext dbContext)
        {
            JobEntity jobEntity = new JobEntity();
            jobEntity.Name = Guid.NewGuid().ToString();

            string sql = @"INSERT INTO Jobs (Id, Name, Created) VALUES (@Id, @Name, @Created)";
            dbContext.ExecuteNonQuery(sql, jobEntity);

            return jobEntity;

        }
    }
}
