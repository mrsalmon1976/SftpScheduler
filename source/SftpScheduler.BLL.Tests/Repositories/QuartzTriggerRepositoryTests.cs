using NUnit.Framework;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Utility;
using System;
using System.IO;
using System.Linq;

namespace SftpScheduler.BLL.Tests.Repositories
{
    [TestFixture]
    public class QuartzTriggerRepositoryTests
    {
        private const string SchedulerName = "SftpScheduler";
        private const string TransferGroup = "TransferJob";

        private string _dbPath = null!;
        private IDbContextFactory _factory = null!;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QuartzTriggerRepositoryTests.db");
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }

            // Create the real Quartz schema the application uses.
            new SQLiteDbMigrator(new DbContextFactory(_dbPath, _dbPath), new ResourceUtils()).InitialiseQuartzDb(_dbPath);
            _factory = new DbContextFactory(_dbPath, _dbPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }

        private void InsertTrigger(IDbContext dbContext, string scheduler, string group, string triggerName, long? nextFireTimeTicks)
        {
            // Quartz's own job detail row is needed for the foreign key.
            dbContext.ExecuteNonQueryAsync(@"INSERT OR IGNORE INTO QRTZ_JOB_DETAILS
                (SCHED_NAME, JOB_NAME, JOB_GROUP, JOB_CLASS_NAME, IS_DURABLE, IS_NONCONCURRENT, IS_UPDATE_DATA, REQUESTS_RECOVERY)
                VALUES (@Scheduler, @JobName, @Group, 'TransferJob', 0, 0, 0, 0)",
                new { Scheduler = scheduler, JobName = triggerName, Group = group }).GetAwaiter().GetResult();

            dbContext.ExecuteNonQueryAsync(@"INSERT INTO QRTZ_TRIGGERS
                (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP, JOB_NAME, JOB_GROUP, NEXT_FIRE_TIME, TRIGGER_STATE, TRIGGER_TYPE, START_TIME)
                VALUES (@Scheduler, @TriggerName, @Group, @JobName, @Group, @NextFireTime, 'WAITING', 'CRON', 0)",
                new { Scheduler = scheduler, TriggerName = triggerName, Group = group, JobName = triggerName, NextFireTime = nextFireTimeTicks }).GetAwaiter().GetResult();
        }

        [Test]
        public void GetAllByGroupAsync_Integration_ReturnsOnlyMatchingSchedulerAndGroup()
        {
            long ticks = new DateTime(2030, 1, 2, 3, 4, 5, DateTimeKind.Utc).Ticks;

            using (IDbContext dbContext = _factory.GetQuartzDbContext())
            {
                InsertTrigger(dbContext, SchedulerName, TransferGroup, "TransferTrigger.1", ticks);
                InsertTrigger(dbContext, SchedulerName, TransferGroup, "TransferTrigger.2", null);
                InsertTrigger(dbContext, SchedulerName, "DigestJob", "DigestTrigger", ticks);
                InsertTrigger(dbContext, "OtherScheduler", TransferGroup, "TransferTrigger.3", ticks);
            }

            using (IDbContext dbContext = _factory.GetQuartzDbContext())
            {
                QuartzTriggerEntity[] result = new QuartzTriggerRepository()
                    .GetAllByGroupAsync(dbContext, SchedulerName, TransferGroup).GetAwaiter().GetResult()
                    .ToArray();

                Assert.That(result.Length, Is.EqualTo(2));

                QuartzTriggerEntity withTime = result.Single(x => x.TriggerName == "TransferTrigger.1");
                Assert.That(withTime.NextFireTimeTicks, Is.EqualTo(ticks));

                QuartzTriggerEntity withoutTime = result.Single(x => x.TriggerName == "TransferTrigger.2");
                Assert.That(withoutTime.NextFireTimeTicks, Is.Null);
            }
        }

        [Test]
        public void GetAllByGroupAsync_NoTriggers_ReturnsEmpty()
        {
            using (IDbContext dbContext = _factory.GetQuartzDbContext())
            {
                var result = new QuartzTriggerRepository().GetAllByGroupAsync(dbContext, SchedulerName, TransferGroup).GetAwaiter().GetResult();

                Assert.That(result.Any(), Is.False);
            }
        }
    }
}
