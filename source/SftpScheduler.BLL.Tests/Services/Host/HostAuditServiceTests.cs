using NUnit.Framework;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Services.Host;
using SftpScheduler.BLL.Utility;
using SftpScheduler.Test.Common;

namespace SftpScheduler.BLL.Tests.Services.Host
{
    [TestFixture]
    public class HostAuditServiceTests
    {

        [Test]
        public void CompareHosts_NoPropertiesChanged_NoAuditRecordCreated()
        {
            // setup
            HostEntity hostEntityOld = new SubstituteBuilder<HostEntity>().WithRandomProperties().WithProperty(x => x.Password, String.Empty).Build();
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
            HostEntity hostEntityOld = new SubstituteBuilder<HostEntity>().WithRandomProperties().WithProperty(x => x.Password, String.Empty).Build();
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
            HostEntity hostEntityOld = new SubstituteBuilder<HostEntity>().WithRandomProperties().WithProperty(x => x.Password, String.Empty).Build();
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
            HostEntity hostEntityOld = new SubstituteBuilder<HostEntity>().WithRandomProperties().WithProperty(x => x.Password, String.Empty).Build();
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
            HostEntity hostEntityOld = new SubstituteBuilder<HostEntity>().WithRandomProperties().WithProperty(x => x.Password, String.Empty).Build();
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
            HostEntity hostEntityOld = new SubstituteBuilder<HostEntity>().WithRandomProperties().Build();
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
            HostEntity hostEntityOld = new SubstituteBuilder<HostEntity>().WithRandomProperties().WithProperty(x => x.Password, String.Empty).Build();
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
        public void CompareHosts_ProtocolChanged_AuditRecordCreated()
        {
            // setup
            HostEntity hostEntityOld = new SubstituteBuilder<HostEntity>().WithRandomProperties().WithProperty(x => x.Protocol, BLL.Data.TransferProtocol.Sftp).Build();
            HostEntity hostEntityNew = ObjectUtils.Clone<HostEntity>(hostEntityOld)!;
            hostEntityNew.Protocol = TransferProtocol.Ftp;
            hostEntityNew.Password = String.Empty;
            string userName = Guid.NewGuid().ToString();

            // execute 
            HostAuditService hostAuditService = new HostAuditService();
            var result = hostAuditService.CompareHosts(hostEntityOld, hostEntityNew, userName);

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), hostEntityOld.Id, "Protocol", TransferProtocol.Sftp.ToString().ToUpper(), TransferProtocol.Ftp.ToString().ToUpper());
        }

        [Test]
        public void CompareHosts_FtpsModeChanged_AuditRecordCreated()
        {
            // setup
            HostEntity hostEntityOld = new SubstituteBuilder<HostEntity>().WithRandomProperties().WithProperty(x => x.FtpsMode, FtpsMode.Implicit).Build();
            HostEntity hostEntityNew = ObjectUtils.Clone<HostEntity>(hostEntityOld)!;
            hostEntityNew.FtpsMode = FtpsMode.Explicit;
            hostEntityNew.Password = String.Empty;
            string userName = Guid.NewGuid().ToString();

            // execute 
            HostAuditService hostAuditService = new HostAuditService();
            var result = hostAuditService.CompareHosts(hostEntityOld, hostEntityNew, userName);

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            AssertAuditLogProperties(result.Single(), hostEntityOld.Id, "FtpsMode", FtpsMode.Implicit.ToString(), FtpsMode.Explicit.ToString());
        }

        [Test]
        public void CompareHosts_AllPropertiesChanged_AuditRecordsCreated()
        {
            // setup
            string userName = Guid.NewGuid().ToString();
            HostEntity hostEntityOld = new SubstituteBuilder<HostEntity>()
                .WithRandomProperties()
                .WithProperty(x => x.Protocol, TransferProtocol.Ftp)
                .WithProperty(x => x.FtpsMode, FtpsMode.Explicit)
                .Build();
            HostEntity hostEntityNew = new SubstituteBuilder<HostEntity>()
                .WithProperty(x => x.Name, hostEntityOld.Name + "a")
                .WithProperty(x => x.Host, hostEntityOld.Host + "a")
                .WithProperty(x => x.Port, (hostEntityOld.Port ?? 0) + 1)
                .WithProperty(x => x.Username, hostEntityOld.Username + "a")
                .WithProperty(x => x.Password, hostEntityOld.Password + "a")
                .WithProperty(x => x.KeyFingerprint, hostEntityOld.KeyFingerprint+ "a")
                .WithProperty(x => x.Protocol, TransferProtocol.Sftp)
                .WithProperty(x => x.FtpsMode, FtpsMode.Implicit)
                .Build();

            // execute 
            HostAuditService hostAuditService = new HostAuditService();
            var result = hostAuditService.CompareHosts(hostEntityOld, hostEntityNew, userName);

            // assert
            Assert.That(result.Count(), Is.EqualTo(8));
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "Name"), hostEntityOld.Id, "Name", hostEntityOld.Name, hostEntityNew.Name);
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "Host"), hostEntityOld.Id, "Host", hostEntityOld.Host, hostEntityNew.Host);
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "Port"), hostEntityOld.Id, "Port", (hostEntityOld.Port ?? 0).ToString(), (hostEntityNew.Port ?? -1).ToString());
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "Username"), hostEntityOld.Id, "Username", hostEntityOld.Username!, hostEntityNew.Username!);
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "Password"), hostEntityOld.Id, "Password", HostAuditService.PasswordMask, HostAuditService.PasswordMask);
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "KeyFingerprint"), hostEntityOld.Id, "KeyFingerprint", hostEntityOld.KeyFingerprint, hostEntityNew.KeyFingerprint);
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "Protocol"), hostEntityOld.Id, "Protocol", hostEntityOld.Protocol.ToString().ToUpper(), hostEntityNew.Protocol.ToString().ToUpper());
            AssertAuditLogProperties(result.Single(x => x.PropertyName == "FtpsMode"), hostEntityOld.Id, "FtpsMode", hostEntityOld.FtpsMode.ToString(), hostEntityNew.FtpsMode.ToString());
        }

        [Test]
        public void CompareHosts_PropertiesChanged_UserIdCorrectlySet()
        {
            // setup
            string userName = Guid.NewGuid().ToString();
            HostEntity hostEntityOld = new SubstituteBuilder<HostEntity>().WithRandomProperties().WithProperty(x => x.Protocol, TransferProtocol.Sftp).Build();
            HostEntity hostEntityNew = new SubstituteBuilder<HostEntity>()
                .WithProperty(x => x.Name, Guid.NewGuid().ToString())
                .WithProperty(x => x.Host, Guid.NewGuid().ToString())
                .WithProperty(x => x.Port, (hostEntityOld.Port ?? 0) + 1)
                .WithProperty(x => x.Username, Guid.NewGuid().ToString())
                .WithProperty(x => x.Password, Guid.NewGuid().ToString())
                .WithProperty(x => x.KeyFingerprint, Guid.NewGuid().ToString())
                .WithProperty(x => x.Protocol, TransferProtocol.Ftp)
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
