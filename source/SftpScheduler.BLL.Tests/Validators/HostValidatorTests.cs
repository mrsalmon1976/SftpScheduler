using NUnit.Framework;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Validators;
using SftpScheduler.Test.Common;
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
            var hostEntity = CreateValidHostBuilder().Build();
            HostValidator hostValidator = new HostValidator();
            var validationResult = hostValidator.Validate(hostEntity);
            Assert.That(validationResult.IsValid);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public void Validate_InValidObjectWithNoName_ExceptionThrown(string name)
        {
            var hostEntity = CreateValidHostBuilder().WithProperty(x => x.Name, name).Build();
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
            var hostEntity = CreateValidHostBuilder().WithProperty(x => x.Host, host).Build();
            HostValidator hostValidator = new HostValidator();
            var validationResult = hostValidator.Validate(hostEntity);
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }

        [TestCase(-1)]
        [TestCase(65536)]
        [TestCase(101010)]
        public void Validate_InValidObjectWithInvalidPort_ExceptionThrown(int port)
        {
            var hostEntity = CreateValidHostBuilder().WithProperty(x => x.Port, port).Build();
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
