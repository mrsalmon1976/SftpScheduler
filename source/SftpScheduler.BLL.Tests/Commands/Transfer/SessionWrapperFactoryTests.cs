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
    }
}
