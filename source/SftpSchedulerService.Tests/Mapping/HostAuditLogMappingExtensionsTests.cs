using SftpScheduler.BLL.Models;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Mapping;
using SftpSchedulerService.Models.HostAuditLog;

namespace SftpSchedulerService.Tests.Mapping
{
    [TestFixture]
    public class HostAuditLogMappingExtensionsTests
    {
        [Test]
        public void ToViewModel_MapsAllProperties()
        {
            var entity = new SubstituteBuilder<HostAuditLogEntity>().WithRandomProperties().Build();

            HostAuditLogViewModel result = entity.ToViewModel();

            Assert.That(result.Id, Is.EqualTo(entity.Id));
            Assert.That(result.HostId, Is.EqualTo(entity.HostId));
            Assert.That(result.PropertyName, Is.EqualTo(entity.PropertyName));
            Assert.That(result.FromValue, Is.EqualTo(entity.FromValue));
            Assert.That(result.ToValue, Is.EqualTo(entity.ToValue));
            Assert.That(result.UserName, Is.EqualTo(entity.UserName));
            Assert.That(result.Created, Is.EqualTo(entity.Created));
        }
    }
}
