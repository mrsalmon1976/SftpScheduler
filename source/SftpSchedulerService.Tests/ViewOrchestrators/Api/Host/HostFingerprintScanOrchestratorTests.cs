using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SftpScheduler.BLL.Commands.Transfer;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Validators;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Models.Host;
using SftpSchedulerService.ViewOrchestrators.Api.Host;

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Host
{
    [TestFixture]
    public class HostFingerprintScanOrchestratorTests
    {
        [Test]
        public void Execute_HostInvalid_ReturnsBadRequest()
        {
            HostViewModel hostViewModel = new SubstituteBuilder<HostViewModel>().WithRandomProperties().Build();
            IHostValidator hostValidator = Substitute.For<IHostValidator>();
            ValidationResult validationResult = new ValidationResult("error");
            hostValidator.Validate(Arg.Any<HostEntity>()).Returns(validationResult);

            // execute
            HostFingerprintScanOrchestrator orchestrator = CreateOrchestrator(hostValidator);
            IActionResult result = orchestrator.Execute(hostViewModel).GetAwaiter().GetResult();

            // assert
            hostValidator.Received(1).Validate(Arg.Any<HostEntity>());

            BadRequestObjectResult badRequestResult = (result as BadRequestObjectResult)!;
            Assert.That(badRequestResult, Is.Not.Null);

            ValidationResult validation = (badRequestResult.Value as ValidationResult)!;
            Assert.That(validation, Is.Not.Null);
            Assert.That(validation.IsValid, Is.False);
            Assert.That(validation.ErrorMessages.Count, Is.EqualTo(1));

        }

        [Test]
        public void Execute_ScanThrowsError_ReturnsBadRequest()
        {
            HostViewModel hostViewModel = new SubstituteBuilder<HostViewModel>().WithRandomProperties().Build();
            IHostValidator hostValidator = Substitute.For<IHostValidator>();
            hostValidator.Validate(Arg.Any<HostEntity>()).Returns(new ValidationResult());

            ISessionWrapperFactory sessionWrapperFactory = Substitute.For<ISessionWrapperFactory>();
            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapper.ScanFingerprint(Arg.Any<string>()).Throws<Exception>();
            sessionWrapperFactory.CreateSession(Arg.Any<HostEntity>()).Returns(sessionWrapper);

            // execute
            HostFingerprintScanOrchestrator orchestrator = CreateOrchestrator(hostValidator, sessionWrapperFactory: sessionWrapperFactory);
            IActionResult result = orchestrator.Execute(hostViewModel).GetAwaiter().GetResult();

            // assert
            sessionWrapperFactory.Received(1).CreateSession(Arg.Any<HostEntity>());
            sessionWrapper.Received().ScanFingerprint(Arg.Any<string>());

            BadRequestObjectResult badRequestResult = (result as BadRequestObjectResult)!;
            Assert.That(badRequestResult, Is.Not.Null);
        }

        [Test]
        public void Execute_ScanSuccess_ReturnsAlgorithmResults()
        {
            HostViewModel hostViewModel = new SubstituteBuilder<HostViewModel>().WithRandomProperties().Build();
            IHostValidator hostValidator = Substitute.For<IHostValidator>();
            hostValidator.Validate(Arg.Any<HostEntity>()).Returns(new ValidationResult());

            string sha256Fingerprint = "Fingerprint-" + SessionWrapper.AlgorithmSha256;
            string md5Fingerprint = "Fingerprint-" + SessionWrapper.AlgorithmMd5;

            ISessionWrapperFactory sessionWrapperFactory = Substitute.For<ISessionWrapperFactory>();
            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapper.ScanFingerprint(SessionWrapper.AlgorithmSha256).Returns(sha256Fingerprint);
            sessionWrapper.ScanFingerprint(SessionWrapper.AlgorithmMd5).Returns(md5Fingerprint);
            sessionWrapperFactory.CreateSession(Arg.Any<HostEntity>()).Returns(sessionWrapper);

            // execute
            HostFingerprintScanOrchestrator orchestrator = CreateOrchestrator(hostValidator, sessionWrapperFactory: sessionWrapperFactory);
            IActionResult result = orchestrator.Execute(hostViewModel).GetAwaiter().GetResult();

            // assert
            sessionWrapperFactory.Received(1).CreateSession(Arg.Any<HostEntity>());
            sessionWrapper.Received(1).ScanFingerprint(SessionWrapper.AlgorithmSha256);
            sessionWrapper.Received(1).ScanFingerprint(SessionWrapper.AlgorithmMd5);

            OkObjectResult okResult = (result as OkObjectResult)!;
            Assert.That(okResult, Is.Not.Null);

            List<HostKeyFingerprintViewModel> fingerprints = (okResult.Value as List<HostKeyFingerprintViewModel>)!;
            Assert.That(fingerprints, Is.Not.Null);
            Assert.That(fingerprints.Single(x => x.Algorithm == SessionWrapper.AlgorithmSha256 && x.KeyFingerprint == sha256Fingerprint), Is.Not.Null);
            Assert.That(fingerprints.Single(x => x.Algorithm == SessionWrapper.AlgorithmMd5 && x.KeyFingerprint == md5Fingerprint), Is.Not.Null);



        }

        private HostFingerprintScanOrchestrator CreateOrchestrator(IHostValidator hostValidator, ISessionWrapperFactory? sessionWrapperFactory = null, IMapper? mapper = null)
        {
            sessionWrapperFactory ??= Substitute.For<ISessionWrapperFactory>();
            mapper ??= AutoMapperTestHelper.CreateMapper();
            
            ILogger<HostFingerprintScanOrchestrator> logger = Substitute.For<ILogger<HostFingerprintScanOrchestrator>>();
            return new HostFingerprintScanOrchestrator(logger, sessionWrapperFactory, mapper, hostValidator);
        }


    }
}
