using SftpScheduler.BLL.Models;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Mapping;
using SftpSchedulerService.Models.JobAuditLog;

namespace SftpSchedulerService.Tests.Mapping
{
    [TestFixture]
    public class JobAuditLogMappingExtensionsTests
    {
        [Test]
        public void ToViewModel_MapsAllProperties()
        {
            var entity = new SubstituteBuilder<JobAuditLogEntity>().WithRandomProperties().Build();

            JobAuditLogViewModel result = entity.ToViewModel();

            Assert.That(result.Id, Is.EqualTo(entity.Id));
            Assert.That(result.JobId, Is.EqualTo(entity.JobId));
            Assert.That(result.PropertyName, Is.EqualTo(entity.PropertyName));
            Assert.That(result.FromValue, Is.EqualTo(entity.FromValue));
            Assert.That(result.ToValue, Is.EqualTo(entity.ToValue));
            Assert.That(result.UserName, Is.EqualTo(entity.UserName));
            Assert.That(result.Created, Is.EqualTo(entity.Created));
        }
    }
}
