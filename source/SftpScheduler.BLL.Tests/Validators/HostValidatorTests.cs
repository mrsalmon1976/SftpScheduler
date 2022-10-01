using NUnit.Framework;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Tests.Validators
{
    [TestFixture]
    public class HostValidatorTests
    {
        [Test]
        public void Validate_ValidObject_NoValidationExceptionThrown()
        {
            HostEntity hostEntity = EntityTestHelper.CreateHostEntity();
            HostValidator hostValidator = new HostValidator();
            var validationResult = hostValidator.Validate(hostEntity);
            Assert.That(validationResult.IsValid);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public void Validate_InValidObjectWithNoName_ExceptionThrown(string name)
        {
            HostEntity hostEntity = EntityTestHelper.CreateHostEntity();
            hostEntity.Name = name;
            HostValidator hostValidator = new HostValidator();
            var validationResult = hostValidator.Validate(hostEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public void Validate_InValidObjectWithNoHost_ExceptionThrown(string host)
        {
            HostEntity hostEntity = EntityTestHelper.CreateHostEntity();
            hostEntity.Host = host;
            HostValidator hostValidator = new HostValidator();
            var validationResult = hostValidator.Validate(hostEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }

        [TestCase(-1)]
        [TestCase(65354)]
        [TestCase(101010)]
        public void Validate_InValidObjectWithInvalidPort_ExceptionThrown(int port)
        {
            HostEntity hostEntity = EntityTestHelper.CreateHostEntity();
            hostEntity.Port = port;
            HostValidator hostValidator = new HostValidator();
            var validationResult = hostValidator.Validate(hostEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public void Validate_InValidObjectWithNoUsername_ExceptionThrown(string userName)
        {
            HostEntity hostEntity = EntityTestHelper.CreateHostEntity();
            hostEntity.Username = userName;
            HostValidator hostValidator = new HostValidator();
            var validationResult = hostValidator.Validate(hostEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("12345")]
        public void Validate_InValidObjectWithInvalidPassword_ExceptionThrown(string password)
        {
            HostEntity hostEntity = EntityTestHelper.CreateHostEntity();
            hostEntity.Password = password;
            HostValidator hostValidator = new HostValidator();
            var validationResult = hostValidator.Validate(hostEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }


        [Test]
        public void Validate_InValidObject_ExceptionThrown()
        {
            HostEntity hostEntity = new HostEntity();

            HostValidator hostValidator = new HostValidator();
            var validationResult = hostValidator.Validate(hostEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(5));
        }

    }

}
