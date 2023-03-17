using NUnit.Framework;
using SftpScheduler.BLL.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Tests.DataAnnotations
{
    [TestFixture]
    public class CronScheduleAttributeTests
    {
        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        public void IsValid_ScheduleIsNullOrWhitespace_ReturnsFalse(string schedule)
        {
            CronScheduleAttribute cronScheduleAttribute = new CronScheduleAttribute();
            bool result = cronScheduleAttribute.IsValid(schedule);
            Assert.IsFalse(result);

        }

        [TestCase("0 * * ? * * *")]
        [TestCase("0 * * 1,14,15 FEB,JUL ? *")]
        public void IsValid_ValidCronSchedule_ReturnsTrue(string schedule)
        {
            CronScheduleAttribute cronScheduleAttribute = new CronScheduleAttribute();
            bool result = cronScheduleAttribute.IsValid(schedule);
            Assert.IsTrue(result);

        }

        [Test]
        public void IsValid_InvalidCronSchedule_ReturnsFalse()
        {
            CronScheduleAttribute cronScheduleAttribute = new CronScheduleAttribute();
            bool result = cronScheduleAttribute.IsValid("test");
            Assert.IsFalse(result);

        }

    }
}
