using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.Transfer;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Validators;
using SftpSchedulerService.Models.Host;

namespace SftpSchedulerService.ViewOrchestrators.Api.Host
{
    public class HostFingerprintScanOrchestrator
    {
        private readonly ILogger<HostFingerprintScanOrchestrator> _logger;
        private readonly ISessionWrapperFactory _sessionWrapperFactory;
        private readonly IMapper _mapper;
        private readonly HostValidator _hostValidator;

        public HostFingerprintScanOrchestrator(ILogger<HostFingerprintScanOrchestrator> logger, ISessionWrapperFactory sessionWrapperFactory, IMapper mapper, HostValidator hostValidator)
        {
            _logger = logger;
            _sessionWrapperFactory = sessionWrapperFactory;
            _mapper = mapper;
            _hostValidator = hostValidator;
        }

        public async Task<IActionResult> Execute(HostViewModel hostViewModel)
        {
            HostEntity hostEntity = _mapper.Map<HostEntity>(hostViewModel);
            ValidationResult validationResult = _hostValidator.Validate(hostEntity);
            if (!validationResult.IsValid) 
            {
                _logger.LogInformation("Scanning host '{hostName}' aborted - invalid host information", hostEntity.Host);
                return new BadRequestObjectResult(validationResult);
            }

            hostEntity.Password = String.Empty;

            ISessionWrapper sessionWrapper = _sessionWrapperFactory.CreateSession(hostEntity);

            try
            {
                _logger.LogInformation("Scanning host '{hostName}' for key fingerprints", hostEntity.Host);
                List<HostKeyFingerprintViewModel> result = new List<HostKeyFingerprintViewModel>();
                result.Add(this.ScanHost(sessionWrapper, SessionWrapper.AlgorithmSha256));
                result.Add(this.ScanHost(sessionWrapper, SessionWrapper.AlgorithmMd5));
                return new OkObjectResult(result);

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return new BadRequestObjectResult(new ValidationResult(ex.Message));
            }
        }

        private HostKeyFingerprintViewModel ScanHost(ISessionWrapper sessionWrapper, string algorithm)
        {
            string keyFingerprint = sessionWrapper.ScanFingerprint(algorithm);
            return new HostKeyFingerprintViewModel(algorithm, keyFingerprint);
        }
    }
}
