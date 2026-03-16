using SftpScheduler.BLL.Models;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Mapping;
using SftpSchedulerService.Models.Host;

namespace SftpSchedulerService.Tests.Mapping
{
    [TestFixture]
    public class HostMappingExtensionsTests
    {
        [Test]
        public void ToViewModel_MapsAllProperties()
        {
            var hostEntity = new SubstituteBuilder<HostEntity>().WithRandomProperties().Build();

            HostViewModel result = hostEntity.ToViewModel();

            Assert.That(result.Id, Is.EqualTo(hostEntity.Id));
            Assert.That(result.Name, Is.EqualTo(hostEntity.Name));
            Assert.That(result.Host, Is.EqualTo(hostEntity.Host));
            Assert.That(result.Port, Is.EqualTo(hostEntity.Port));
            Assert.That(result.UserName, Is.EqualTo(hostEntity.Username));
            Assert.That(result.Password, Is.EqualTo(hostEntity.Password));
            Assert.That(result.KeyFingerprint, Is.EqualTo(hostEntity.KeyFingerprint));
            Assert.That(result.Protocol, Is.EqualTo(hostEntity.Protocol));
            Assert.That(result.FtpsMode, Is.EqualTo(hostEntity.FtpsMode));
        }

        [Test]
        public void ToViewModel_NullPort_DefaultsToZero()
        {
            var hostEntity = new SubstituteBuilder<HostEntity>().WithRandomProperties().WithProperty(x => x.Port, null).Build();

            HostViewModel result = hostEntity.ToViewModel();

            Assert.That(result.Port, Is.EqualTo(0));
        }

        [Test]
        public void ToEntity_MapsAllProperties()
        {
            var hostViewModel = new SubstituteBuilder<HostViewModel>().WithRandomProperties().Build();

            HostEntity result = hostViewModel.ToEntity();

            Assert.That(result.Id, Is.EqualTo(hostViewModel.Id));
            Assert.That(result.Name, Is.EqualTo(hostViewModel.Name));
            Assert.That(result.Host, Is.EqualTo(hostViewModel.Host));
            Assert.That(result.Port, Is.EqualTo(hostViewModel.Port));
            Assert.That(result.Username, Is.EqualTo(hostViewModel.UserName));
            Assert.That(result.Password, Is.EqualTo(hostViewModel.Password));
            Assert.That(result.KeyFingerprint, Is.EqualTo(hostViewModel.KeyFingerprint));
            Assert.That(result.Protocol, Is.EqualTo(hostViewModel.Protocol));
            Assert.That(result.FtpsMode, Is.EqualTo(hostViewModel.FtpsMode));
        }
    }
}
