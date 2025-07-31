using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.Transfer;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Security;
using SftpScheduler.Test.Common;
using System.Security.Cryptography;
using WinSCP;

namespace SftpScheduler.BLL.Tests.Commands.Transfer
{
    [TestFixture]
    public class SessionWrapperFactoryTests
    {
        [Test]
        public void CreateSession_DecryptsPasswords() 
        {
            string encryptedPassword = Guid.NewGuid().ToString();
            string decryptedPasswrd = Guid.NewGuid().ToString();

            var hostEntity = new SubstituteBuilder<HostEntity>()
                .WithRandomProperties()
                .WithProperty(x => x.Password, encryptedPassword)
                .WithProperty(x => x.Port, RandomData.Number(1, 65535))
                .WithProperty(x => x.KeyFingerprint, String.Empty)
                .Build();

            // set up
            IEncryptionProvider encryptionProvider = Substitute.For<IEncryptionProvider>();
            encryptionProvider.Decrypt(encryptedPassword).Returns(decryptedPasswrd);

            // execute
            ISessionWrapperFactory sessionWrapperFactory = new SessionWrapperFactory(encryptionProvider);
            ISessionWrapper sessionWrapper = sessionWrapperFactory.CreateSession(hostEntity);

            // assert
            encryptionProvider.Received(1).Decrypt(encryptedPassword);
            Assert.That(sessionWrapper.SessionOptions.Password, Is.EqualTo(decryptedPasswrd));

        }

        [Test]
        public void CreateSession_DecryptionFails_ThrowsApplicationException()
        {
            string encryptedPassword = Guid.NewGuid().ToString();
            string decryptedPasswrd = Guid.NewGuid().ToString();

            var hostEntity = new SubstituteBuilder<HostEntity>()
                .WithRandomProperties()
                .WithProperty(x => x.Password, encryptedPassword)
                .WithProperty(x => x.Port, RandomData.Number(1, 65535))
                .Build();

            // set up
            IEncryptionProvider encryptionProvider = Substitute.For<IEncryptionProvider>();
            encryptionProvider.Decrypt(encryptedPassword).Throws(new CryptographicException());

            // execute
            ISessionWrapperFactory sessionWrapperFactory = new SessionWrapperFactory(encryptionProvider);
            try
            {
                sessionWrapperFactory.CreateSession(hostEntity);
                Assert.Fail("ApplicationException not thrown");
            }
            catch (ApplicationException aex)
            {
                Assert.That(aex.Message.StartsWith("Unable to decrypt password"));
            }

            // assert
            encryptionProvider.Received(1).Decrypt(encryptedPassword);
        }


        [TestCase("")]
        [TestCase(null)]
        public void CreateSession_SftpWithNoKeyFingerPrint_SetsPolicyToGiveUpSecurity(string hostKeyFingerprint)
        {
            var hostEntity = new SubstituteBuilder<HostEntity>()
                .WithRandomProperties()
                .WithProperty(x => x.Protocol, TransferProtocol.Sftp)
                .WithProperty(x => x.KeyFingerprint, hostKeyFingerprint)
                .WithProperty(x => x.Port, RandomData.Number(1, 65535))
                .Build();

            // execute
            ISessionWrapperFactory sessionWrapperFactory = new SessionWrapperFactory(Substitute.For<IEncryptionProvider>());
            ISessionWrapper sessionWrapper = sessionWrapperFactory.CreateSession(hostEntity);

            // assert
            Assert.That(sessionWrapper.SessionOptions.SshHostKeyPolicy, Is.EqualTo(SshHostKeyPolicy.GiveUpSecurityAndAcceptAny));
            Assert.That(sessionWrapper.SessionOptions.SshHostKeyFingerprint, Is.Null);

        }

        [Test]
        public void CreateSession_SftpWithWithKeyFingerPrint_CorrectlySetsOnSession()
        {
            const string hostKeyFingerprint = "ssh-rsa 2048 e4:9b:47:5a:fc:09:0e:41:a9:7d:0d:f9:cc:c0:4d:e0";
            var hostEntity = new SubstituteBuilder<HostEntity>()
                .WithRandomProperties()
                .WithProperty(x => x.Protocol, TransferProtocol.Sftp)
                .WithProperty(x => x.KeyFingerprint, hostKeyFingerprint)
                .WithProperty(x => x.Port, RandomData.Number(1, 65535))
                .Build();

            // execute
            ISessionWrapperFactory sessionWrapperFactory = new SessionWrapperFactory(Substitute.For<IEncryptionProvider>());
            ISessionWrapper sessionWrapper = sessionWrapperFactory.CreateSession(hostEntity);

            // assert
            Assert.That(sessionWrapper.SessionOptions.SshHostKeyFingerprint, Is.EqualTo(hostKeyFingerprint));

        }

        [TestCase(TransferProtocol.Ftps)]
        [TestCase(TransferProtocol.Ftp)]
        public void CreateSession_ProtocolNotSftp_DoesNotSetKeyFingerprint(TransferProtocol protocol)
        {
            const string hostKeyFingerprint = "ssh-rsa 2048 e4:9b:47:5a:fc:09:0e:41:a9:7d:0d:f9:cc:c0:4d:e0";
            var hostEntity = new SubstituteBuilder<HostEntity>()
                .WithRandomProperties()
                .WithProperty(x => x.Protocol, protocol)
                .WithProperty(x => x.KeyFingerprint, hostKeyFingerprint)
                .WithProperty(x => x.Port, RandomData.Number(1, 65535))
                .Build();

            // execute
            ISessionWrapperFactory sessionWrapperFactory = new SessionWrapperFactory(Substitute.For<IEncryptionProvider>());
            ISessionWrapper sessionWrapper = sessionWrapperFactory.CreateSession(hostEntity);

            // assert
            Assert.That(sessionWrapper.SessionOptions.SshHostKeyFingerprint, Is.Null);

        }

        [TestCase(TransferProtocol.Ftps, Protocol.Ftp)]
        [TestCase(TransferProtocol.Ftp, Protocol.Ftp)]
        [TestCase(TransferProtocol.Sftp, Protocol.Sftp)]
        public void CreateSession_Protocol_MapsCorrectly(TransferProtocol transferProtcool, Protocol expectedProtocol)
        {
            var hostEntity = CreateValidHostBuilder()
                .WithProperty(x => x.Protocol, transferProtcool)
                .Build();

            // execute
            ISessionWrapperFactory sessionWrapperFactory = new SessionWrapperFactory(Substitute.For<IEncryptionProvider>());
            ISessionWrapper sessionWrapper = sessionWrapperFactory.CreateSession(hostEntity);

            // assert
            Assert.That(sessionWrapper.SessionOptions.Protocol, Is.EqualTo(expectedProtocol));

        }

        [TestCase(FtpsMode.None, FtpSecure.None)]
        [TestCase(FtpsMode.Implicit, FtpSecure.Implicit)]
        [TestCase(FtpsMode.Explicit, FtpSecure.Explicit)]
        public void CreateSession_FtpsProtocolSet_MapsFtpsModeCorrectly(FtpsMode ftpsMode, FtpSecure expectedMode)
        {
            var hostEntity = CreateValidHostBuilder()
                .WithProperty(x => x.Protocol, TransferProtocol.Ftps)
                .WithProperty(x => x.FtpsMode, ftpsMode)
                .Build();

            // execute
            ISessionWrapperFactory sessionWrapperFactory = new SessionWrapperFactory(Substitute.For<IEncryptionProvider>());
            ISessionWrapper sessionWrapper = sessionWrapperFactory.CreateSession(hostEntity);

            // assert
            Assert.That(sessionWrapper.SessionOptions.FtpSecure, Is.EqualTo(expectedMode));

        }

        private SubstituteBuilder<HostEntity> CreateValidHostBuilder()
        {
            return new SubstituteBuilder<HostEntity>()
                .WithRandomProperties()
                .WithProperty(x => x.KeyFingerprint, String.Empty)
                .WithProperty(x => x.Port, RandomData.Number(1, 65535))
                .WithProperty(x => x.Host, RandomData.IPAddress().ToString());
        }


    }
}
