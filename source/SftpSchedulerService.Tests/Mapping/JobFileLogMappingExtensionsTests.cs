using SftpScheduler.BLL.Models;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Mapping;
using SftpSchedulerService.Models.Job;

namespace SftpSchedulerService.Tests.Mapping
{
    [TestFixture]
    public class JobFileLogMappingExtensionsTests
    {
        [Test]
        public void ToViewModel_MapsAllProperties()
        {
            var entity = new SubstituteBuilder<JobFileLogEntity>().WithRandomProperties().Build();

            JobFileLogViewModel result = entity.ToViewModel();

            Assert.That(result.Id, Is.EqualTo(entity.Id));
            Assert.That(result.JobId, Is.EqualTo(entity.JobId));
            Assert.That(result.FileName, Is.EqualTo(entity.FileName));
            Assert.That(result.FileLength, Is.EqualTo(entity.FileLength));
            Assert.That(result.StartDate, Is.EqualTo(entity.StartDate));
            Assert.That(result.EndDate, Is.EqualTo(entity.EndDate));
        }

        [Test]
        public void ToViewModel_NullEndDate_MapsAsNull()
        {
            var entity = new SubstituteBuilder<JobFileLogEntity>().WithRandomProperties().Build();
            entity.EndDate = null;

            JobFileLogViewModel result = entity.ToViewModel();

            Assert.That(result.EndDate, Is.Null);
        }
    }
}
