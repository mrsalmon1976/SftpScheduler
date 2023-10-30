using NUnit.Framework;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Services.Host;
using SftpScheduler.BLL.Tests.Builders.Models;
using SftpScheduler.BLL.Utility;

namespace SftpScheduler.BLL.Tests.Services.Host
{
    [TestFixture]
    public class HostAuditServiceTests
    {

        [Test]
        public void CompareHosts_NoPropertiesChanged_NoAuditRecordCreated()
        {
            // setup
            HostEntity hostEntityOld = new HostEntityBuilder().WithRandomProperties().WithPassword(String.Empty).Build();
            HostEntity hostEntityNew = ObjectUtils.Clone<HostEntity>(hostEntityOld)!;
            string userName = Guid.NewGuid().ToString();

            // execute 
            HostAuditService hostAuditService = new HostAuditService();
            var result = hostAuditService.CompareHosts(hostEntityOld, hostEntityNew, userName);

            // assert
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public void CompareHosts_NameChanged_AuditRecordCreated()
        {
            // setup
            HostEntity hostEntityOld = new HostEntityBuilder().WithRandomProperties().WithPassword(String.Empty).Build();
            HostEntity hostEntityNew = ObjectUtils.Clone<HostEntity>(hostEntityOld)!;
            hostEntityNew.Name = Guid.NewGuid().ToString();
            string userName = Guid.NewGuid().ToString();

            // execute 
            HostAuditService hostAuditService = new HostAuditService();
            var result = hostAuditService.CompareHosts(hostEntityOld, hostEntityNew, userName);

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), hostEntityOld.Id, "Name", hostEntityOld.Name, hostEntityNew.Name);
        }

        [Test]
        public void CompareHosts_HostChanged_AuditRecordCreated()
        {
            // setup
            HostEntity hostEntityOld = new HostEntityBuilder().WithRandomProperties().WithPassword(String.Empty).Build();
            HostEntity hostEntityNew = ObjectUtils.Clone<HostEntity>(hostEntityOld)!;
            hostEntityNew.Host = Guid.NewGuid().ToString();
            string userName = Guid.NewGuid().ToString();

            // execute 
            HostAuditService hostAuditService = new HostAuditService();
            var result = hostAuditService.CompareHosts(hostEntityOld, hostEntityNew, userName);

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), hostEntityOld.Id, "Host", hostEntityOld.Host, hostEntityNew.Host);
        }

        [Test]
        public void CompareHosts_PortChanged_AuditRecordCreated()
        {
            // setup
            HostEntity hostEntityOld = new HostEntityBuilder().WithRandomProperties().WithPassword(String.Empty).Build();
            HostEntity hostEntityNew = ObjectUtils.Clone<HostEntity>(hostEntityOld)!;
            hostEntityNew.Port = hostEntityOld.Port + Faker.RandomNumber.Next(1, 10);
            string userName = Guid.NewGuid().ToString();

            // execute 
            HostAuditService hostAuditService = new HostAuditService();
            var result = hostAuditService.CompareHosts(hostEntityOld, hostEntityNew, userName);

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), hostEntityOld.Id, "Port", (hostEntityOld.Port ?? 0).ToString(), (hostEntityNew.Port ?? -1).ToString());
        }

        [Test]
        public void CompareHosts_UsernameChanged_AuditRecordCreated()
        {
            // setup
            HostEntity hostEntityOld = new HostEntityBuilder().WithRandomProperties().WithPassword(String.Empty).Build();
            HostEntity hostEntityNew = ObjectUtils.Clone<HostEntity>(hostEntityOld)!;
            hostEntityNew.Username = Guid.NewGuid().ToString();
            string userName = Guid.NewGuid().ToString();

            // execute 
            HostAuditService hostAuditService = new HostAuditService();
            var result = hostAuditService.CompareHosts(hostEntityOld, hostEntityNew, userName);

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), hostEntityOld.Id, "Username", hostEntityOld.Username!, hostEntityNew.Username);
        }

        [Test]
        public void CompareHosts_PasswordChanged_AuditRecordCreated()
        {
            // setup
            HostEntity hostEntityOld = new HostEntityBuilder().WithRandomProperties().Build();
            HostEntity hostEntityNew = ObjectUtils.Clone<HostEntity>(hostEntityOld)!;
            hostEntityNew.Password = Guid.NewGuid().ToString();
            string userName = Guid.NewGuid().ToString();

            // execute 
            HostAuditService hostAuditService = new HostAuditService();
            var result = hostAuditService.CompareHosts(hostEntityOld, hostEntityNew, userName);

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), hostEntityOld.Id, "Password", HostAuditService.PasswordMask, HostAuditService.PasswordMask);
        }

        [Test]
        public void CompareHosts_KeyFingerprintChanged_AuditRecordCreated()
        {
            // setup
            HostEntity hostEntityOld = new HostEntityBuilder().WithRandomProperties().WithPassword(String.Empty).Build();
            HostEntity hostEntityNew = ObjectUtils.Clone<HostEntity>(hostEntityOld)!;
            hostEntityNew.KeyFingerprint = Guid.NewGuid().ToString();
            string userName = Guid.NewGuid().ToString();

            // execute 
            HostAuditService hostAuditService = new HostAuditService();
            var result = hostAuditService.CompareHosts(hostEntityOld, hostEntityNew, userName);

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), hostEntityOld.Id, "KeyFingerprint", hostEntityOld.KeyFingerprint, hostEntityNew.KeyFingerprint);
        }

        [Test]
        public void CompareHosts_AllPropertiesChanged_AuditRecordsCreated()
        {
            // setup
            string userName = Guid.NewGuid().ToString();
            HostEntity hostEntityOld = new HostEntityBuilder().WithRandomProperties().Build();
            HostEntity hostEntityNew = new HostEntityBuilder()
                .WithName(Guid.NewGuid().ToString())
                .WithHost(Guid.NewGuid().ToString())
                .WithPort((hostEntityOld.Port ?? 0) + 1)
                .WithUsername(Guid.NewGuid().ToString())
                .WithPassword(Guid.NewGuid().ToString())
                .WithKeyFingerprint(Guid.NewGuid().ToString())
                .Build();

            // execute 
            HostAuditService hostAuditService = new HostAuditService();
            var result = hostAuditService.CompareHosts(hostEntityOld, hostEntityNew, userName);

            // assert
            Assert.That(result.Count(), Is.EqualTo(6));
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "Name"), hostEntityOld.Id, "Name", hostEntityOld.Name, hostEntityNew.Name);
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "Host"), hostEntityOld.Id, "Host", hostEntityOld.Host, hostEntityNew.Host);
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "Port"), hostEntityOld.Id, "Port", (hostEntityOld.Port ?? 0).ToString(), (hostEntityNew.Port ?? -1).ToString());
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "Username"), hostEntityOld.Id, "Username", hostEntityOld.Username!, hostEntityNew.Username!);
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "Password"), hostEntityOld.Id, "Password", HostAuditService.PasswordMask, HostAuditService.PasswordMask);
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "KeyFingerprint"), hostEntityOld.Id, "KeyFingerprint", hostEntityOld.KeyFingerprint, hostEntityNew.KeyFingerprint);
        }

        [Test]
        public void CompareHosts_PropertiesChanged_UserIdCorrectlySet()
        {
            // setup
            string userName = Guid.NewGuid().ToString();
            HostEntity hostEntityOld = new HostEntityBuilder().WithRandomProperties().Build();
            HostEntity hostEntityNew = new HostEntityBuilder()
                .WithName(Guid.NewGuid().ToString())
                .WithHost(Guid.NewGuid().ToString())
                .WithPort((hostEntityOld.Port ?? 0) + 1)
                .WithUsername(Guid.NewGuid().ToString())
                .WithPassword(Guid.NewGuid().ToString())
                .WithKeyFingerprint(Guid.NewGuid().ToString())
                .Build();

            // execute 
            HostAuditService hostAuditService = new HostAuditService();
            var auditLogEntities = hostAuditService.CompareHosts(hostEntityOld, hostEntityNew, userName);

            // assert
            Assert.That(auditLogEntities.Count(), Is.GreaterThan(1));
            foreach (var auditLogEntity in auditLogEntities)
            {
                Assert.That(auditLogEntity.UserName, Is.EqualTo(userName));
            }
        }

        private void AssertAuditLogProperties(HostAuditLogEntity hostAuditLogEntity, int hostId, string propertyName, string fromValue, string toValue)
        {
            Assert.That(hostAuditLogEntity.HostId, Is.EqualTo(hostId));
            Assert.That(hostAuditLogEntity.PropertyName, Is.EqualTo(propertyName));
            Assert.That(hostAuditLogEntity.FromValue, Is.EqualTo(fromValue));
            Assert.That(hostAuditLogEntity.ToValue, Is.EqualTo(toValue));
        }

    }
}
