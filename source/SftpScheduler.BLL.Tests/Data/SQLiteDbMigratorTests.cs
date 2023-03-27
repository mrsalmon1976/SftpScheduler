using NUnit.Framework;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Tests.Data
{
    [TestFixture]
    public class SQLiteDbMigratorTests
    {
        private string _dbPath = "";
        private string _quartzPath = "";

        [SetUp]
        public void SQLiteDbMigratorTests_SetUp()
        {
            _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SQLiteDbMigratorTests.db");
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }

            _quartzPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SQLiteQuartzTests.db");
            if (File.Exists(_quartzPath))
            {
                File.Delete(_quartzPath);
            }
        }

        [TearDown]
        public void SQLiteDbMigratorTests_TearDown()
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
            if (File.Exists(_quartzPath))
            {
                File.Delete(_quartzPath);
            }
        }

        [Test]
        public void MigrateDbChanges_Integration_DbCreated()
        {
            IDbContextFactory dbContextFactory = new DbContextFactory(_dbPath);
            IDbMigrator dbMigrator = new SQLiteDbMigrator(dbContextFactory, new ResourceUtils());
            dbMigrator.MigrateDbChanges();
            Assert.IsTrue(File.Exists(_dbPath));
        }

        [TestCase("Job")]
        [TestCase("JobLog")]
        [TestCase("Host")]
        [TestCase("AspNetRoleClaims")]
        [TestCase("AspNetRoles")]
        [TestCase("AspNetUsers")]
        [TestCase("AspNetUserClaims")]
        [TestCase("AspNetUserLogins")]
        [TestCase("AspNetUserRoles")]
        [TestCase("AspNetUserTokens")]
        public void MigrateDbChanges_Integration_TableCreated(string tableName)
        {
            IDbContextFactory dbContextFactory = new DbContextFactory(_dbPath);
            IDbMigrator dbMigrator = new SQLiteDbMigrator(dbContextFactory, new ResourceUtils());
            dbMigrator.MigrateDbChanges();

            using (IDbContext dbContext = dbContextFactory.GetDbContext())
            {
                string sql = String.Format("SELECT * from {0} LIMIT 5", tableName);
                dbContext.ExecuteNonQueryAsync(sql).GetAwaiter().GetResult();
            }

        }

        [Test]
        public void InitialiseQuartzDb_Integration_DbCreated()
        {
            IDbContextFactory dbContextFactory = new DbContextFactory(_dbPath);
            IDbMigrator dbMigrator = new SQLiteDbMigrator(dbContextFactory, new ResourceUtils());
            dbMigrator.InitialiseQuartzDb(_quartzPath);
            Assert.IsTrue(File.Exists(_quartzPath));
        }

        [TestCase("QRTZ_JOB_DETAILS")]
        [TestCase("QRTZ_TRIGGERS")]
        public void InitialiseQuartzDb_Integration_TableCreated(string tableName)
        {
            IDbContextFactory dbContextFactory = new DbContextFactory(_dbPath);
            IDbMigrator dbMigrator = new SQLiteDbMigrator(dbContextFactory, new ResourceUtils());
            dbMigrator.InitialiseQuartzDb(_quartzPath);

            using (var dbContext = new SQLiteDbContext(_quartzPath))
            {
                string sql = String.Format("SELECT * from {0} LIMIT 5", tableName);
                dbContext.ExecuteNonQueryAsync(sql).GetAwaiter().GetResult();
            }

        }

    }
}
