using NUnit.Framework;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Tests.Builders.Models;
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
            var hostEntity = new HostEntityBuilder().WithRandomProperties().Build();
            HostValidator hostValidator = new HostValidator();
            var validationResult = hostValidator.Validate(hostEntity);
            Assert.That(validationResult.IsValid);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public void Validate_InValidObjectWithNoName_ExceptionThrown(string name)
        {
            var hostEntity = new HostEntityBuilder().WithRandomProperties().WithName(name).Build();
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
            var hostEntity = new HostEntityBuilder().WithRandomProperties().WithHost(host).Build();
            HostValidator hostValidator = new HostValidator();
            var validationResult = hostValidator.Validate(hostEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(65536)]
        [TestCase(101010)]
        public void Validate_InValidObjectWithInvalidPort_ExceptionThrown(int port)
        {
            var hostEntity = new HostEntityBuilder().WithRandomProperties().WithPort(port).Build();
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
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(3));
        }

    }

}
