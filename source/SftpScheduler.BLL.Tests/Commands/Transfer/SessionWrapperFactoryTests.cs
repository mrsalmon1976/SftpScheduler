using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.Transfer;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Security;
using SftpScheduler.BLL.Tests.Builders.Models;
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

            var hostEntity = new HostEntityBuilder().WithRandomProperties().WithPassword(encryptedPassword).Build();

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

            HostEntity hostEntity = new HostEntityBuilder().WithPassword(encryptedPassword).Build();

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
        public void CreateSession_NoKeyFingerPrint_SetsPolicyToGiveUpSecurity(string hostKeyFingerprint)
        {
            var hostEntity = new HostEntityBuilder().WithRandomProperties().WithKeyFingerprint(hostKeyFingerprint).Build();

            // execute
            ISessionWrapperFactory sessionWrapperFactory = new SessionWrapperFactory(Substitute.For<IEncryptionProvider>());
            ISessionWrapper sessionWrapper = sessionWrapperFactory.CreateSession(hostEntity);

            // assert
            Assert.That(sessionWrapper.SessionOptions.SshHostKeyPolicy, Is.EqualTo(SshHostKeyPolicy.GiveUpSecurityAndAcceptAny));
            Assert.That(sessionWrapper.SessionOptions.SshHostKeyFingerprint, Is.Null);

        }

        [Test]
        public void CreateSession_NoKeyFingerPrint_SetsPolicyToGiveUpSecurity()
        {
            const string hostKeyFingerprint = "ssh-rsa 2048 e4:9b:47:5a:fc:09:0e:41:a9:7d:0d:f9:cc:c0:4d:e0";
            var hostEntity = new HostEntityBuilder().WithRandomProperties().WithKeyFingerprint(hostKeyFingerprint).Build();

            // execute
            ISessionWrapperFactory sessionWrapperFactory = new SessionWrapperFactory(Substitute.For<IEncryptionProvider>());
            ISessionWrapper sessionWrapper = sessionWrapperFactory.CreateSession(hostEntity);

            // assert
            Assert.That(sessionWrapper.SessionOptions.SshHostKeyFingerprint, Is.EqualTo(hostKeyFingerprint));

        }

    }
}
