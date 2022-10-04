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

        [SetUp]
        public void SQLiteDbMigratorTests_SetUp()
        {
            _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SQLiteDbMigratorTests.db");
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }

        [TearDown]
        public void SQLiteDbMigratorTests_TearDown()
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
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
        [TestCase("Host")]
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
    }
}
