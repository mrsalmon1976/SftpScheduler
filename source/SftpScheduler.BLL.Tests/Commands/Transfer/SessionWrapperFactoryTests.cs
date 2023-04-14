using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.Transfer;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            
            HostEntity hostEntity = EntityTestHelper.CreateHostEntity();
            hostEntity.Password = encryptedPassword;
            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();

            // set up
            IPasswordProvider passwordProvider = Substitute.For<IPasswordProvider>();
            passwordProvider.Decrypt(encryptedPassword).Returns(decryptedPasswrd);

            // execute
            ISessionWrapperFactory sessionWrapperFactory = new SessionWrapperFactory(passwordProvider);
            ISessionWrapper sessionWrapper = sessionWrapperFactory.CreateSession(hostEntity, jobEntity);

            // assert
            passwordProvider.Received(1).Decrypt(encryptedPassword);
            Assert.That(sessionWrapper.SessionOptions.Password, Is.EqualTo(decryptedPasswrd));

        }

        [TestCase("")]
        [TestCase(null)]
        public void CreateSession_NoKeyFingerPrint_SetsPolicyToGiveUpSecurity(string hostKeyFingerprint)
        {
            HostEntity hostEntity = EntityTestHelper.CreateHostEntity();
            hostEntity.KeyFingerprint = hostKeyFingerprint;

            // execute
            ISessionWrapperFactory sessionWrapperFactory = new SessionWrapperFactory(Substitute.For<IPasswordProvider>());
            ISessionWrapper sessionWrapper = sessionWrapperFactory.CreateSession(hostEntity, EntityTestHelper.CreateJobEntity());

            // assert
            Assert.That(sessionWrapper.SessionOptions.SshHostKeyPolicy, Is.EqualTo(SshHostKeyPolicy.GiveUpSecurityAndAcceptAny));
            Assert.That(sessionWrapper.SessionOptions.SshHostKeyFingerprint, Is.Null);

        }

        [Test]
        public void CreateSession_NoKeyFingerPrint_SetsPolicyToGiveUpSecurity()
        {
            const string hostKeyFingerprint = "ssh-rsa 2048 e4:9b:47:5a:fc:09:0e:41:a9:7d:0d:f9:cc:c0:4d:e0";
            HostEntity hostEntity = EntityTestHelper.CreateHostEntity();
            hostEntity.KeyFingerprint = hostKeyFingerprint;

            // execute
            ISessionWrapperFactory sessionWrapperFactory = new SessionWrapperFactory(Substitute.For<IPasswordProvider>());
            ISessionWrapper sessionWrapper = sessionWrapperFactory.CreateSession(hostEntity, EntityTestHelper.CreateJobEntity());

            // assert
            Assert.That(sessionWrapper.SessionOptions.SshHostKeyFingerprint, Is.EqualTo(hostKeyFingerprint));

        }

    }
}
