using SftpScheduler.BLL.Models;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Mapping;
using SftpSchedulerService.Models.Job;

namespace SftpSchedulerService.Tests.Mapping
{
    [TestFixture]
    public class JobLogMappingExtensionsTests
    {
        [Test]
        public void ToViewModel_MapsAllProperties()
        {
            var jobLogEntity = new SubstituteBuilder<JobLogEntity>().WithRandomProperties().Build();

            JobLogViewModel result = jobLogEntity.ToViewModel();

            Assert.That(result.Id, Is.EqualTo(jobLogEntity.Id));
            Assert.That(result.JobId, Is.EqualTo(jobLogEntity.JobId));
            Assert.That(result.Progress, Is.EqualTo(jobLogEntity.Progress));
            Assert.That(result.Status, Is.EqualTo(jobLogEntity.Status));
            Assert.That(result.ErrorMessage, Is.EqualTo(jobLogEntity.ErrorMessage));
        }

        [Test]
        public void ToViewModel_StartDateConvertedToLocalTime()
        {
            var jobLogEntity = new SubstituteBuilder<JobLogEntity>()
                .WithRandomProperties()
                .WithProperty(x => x.StartDate, DateTime.UtcNow)
                .Build();

            JobLogViewModel result = jobLogEntity.ToViewModel();

            Assert.That(result.StartDate, Is.EqualTo(jobLogEntity.StartDate.ToLocalTime()));
        }

        [Test]
        public void ToViewModel_EndDateConvertedToLocalTime()
        {
            var endDate = DateTime.UtcNow;
            var jobLogEntity = new SubstituteBuilder<JobLogEntity>()
                .WithRandomProperties()
                .WithProperty(x => x.EndDate, endDate)
                .Build();

            JobLogViewModel result = jobLogEntity.ToViewModel();

            Assert.That(result.EndDate, Is.EqualTo(endDate.ToLocalTime()));
        }

        [Test]
        public void ToViewModel_NullEndDate_RemainsNull()
        {
            var jobLogEntity = new SubstituteBuilder<JobLogEntity>()
                .WithRandomProperties()
                .WithProperty(x => x.EndDate, null)
                .Build();

            JobLogViewModel result = jobLogEntity.ToViewModel();

            Assert.That(result.EndDate, Is.Null);
        }
    }
}
