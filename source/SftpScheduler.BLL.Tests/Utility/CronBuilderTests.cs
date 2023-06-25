using NUnit.Framework;
using SftpScheduler.BLL.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Tests.Utility
{
    [TestFixture]
    public class CronBuilderTests
    {
        [TestCase(-1)]
        [TestCase(24)]
        public void Daily_HoursInvalid_ReturnsNull(int hour)
        {
            // setup 
            CronBuilder cronBuilder = new CronBuilder();

            // execute
            try
            {
                string? result = cronBuilder.Daily(Enumerable.Empty<DayOfWeek>(), hour);
                Assert.Fail("ArgumentOutOfRangeException not thrown");
            }
            catch (ArgumentOutOfRangeException) 
            { 
                // good times
            }

        }

        [TestCase(1)]
        [TestCase(12)]
        public void Daily_NoDays_ReturnsNull(int hour)
        {
            // setup 
            CronBuilder cronBuilder = new CronBuilder();

            // execute
            string? result = cronBuilder.Daily(Enumerable.Empty<DayOfWeek>(), hour);

            // assert
            Assert.That(result, Is.Null);
        }

        [TestCase("0 0 7 ? * TUE *", 7, DayOfWeek.Tuesday)]
        [TestCase("0 0 2 ? * MON,THU *", 2, DayOfWeek.Monday, DayOfWeek.Thursday)]
        [TestCase("0 0 17 ? * SUN,MON,TUE,WED,THU,FRI,SAT *", 17, DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday)]
        public void Daily_DaysSupplied_ReturnsVlidCron(string expectedCron, int hour, params DayOfWeek[] daysOfWeek)
        {
            // setup 
            CronBuilder cronBuilder = new CronBuilder();

            // execute
            string? result = cronBuilder.Daily(daysOfWeek, hour);

            // assert
            Assert.That(result, Is.EqualTo(expectedCron));
        }

    }
}
