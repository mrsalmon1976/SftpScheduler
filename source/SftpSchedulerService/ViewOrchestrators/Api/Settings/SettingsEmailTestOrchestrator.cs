using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Net;
using SftpScheduler.BLL.Validators;
using SftpSchedulerService.Models.Settings;
using System.Net.Mail;

namespace SftpSchedulerService.ViewOrchestrators.Api.Settings
{
    public interface ISettingsEmailTestOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(EmailTestViewModel testEmailViewModel);
    }

    public class SettingsEmailTestOrchestrator : ISettingsEmailTestOrchestrator
    {
        private readonly ILogger<SettingsEmailTestOrchestrator> _logger;
        private readonly ISmtpClientWrapper _smtpClientWrapper;
        private readonly ISmtpHostValidator _smtpHostValidator;
        private readonly IEmailValidator _emailValidator;

        public SettingsEmailTestOrchestrator(ILogger<SettingsEmailTestOrchestrator> logger, ISmtpClientWrapper smtpClientWrapper, ISmtpHostValidator smtpHostValidator, IEmailValidator emailValidator)
        {
            _logger = logger;
            _smtpClientWrapper = smtpClientWrapper;
            _smtpHostValidator = smtpHostValidator;
            _emailValidator = emailValidator;
        }

        public async Task<IActionResult> Execute(EmailTestViewModel testEmailViewModel)
        {
            SmtpHost smtpHost = EmailTestMapper.MapToSmtpHost(testEmailViewModel);
            var result = _smtpHostValidator.Validate(smtpHost);
            if (!result.IsValid)
            {
                return new BadRequestObjectResult(result);
            }
            if (!_emailValidator.IsValidEmail(testEmailViewModel.FromAddress))
            {
                return new BadRequestObjectResult(new ValidationResult("From address is not a valid email address"));
            }
            if (!_emailValidator.IsValidEmail(testEmailViewModel.ToAddress))
            {
                return new BadRequestObjectResult(new ValidationResult("To address is not a valid email address"));
            }

            MailMessage mailMessage = EmailTestMapper.MapToMailMessage(testEmailViewModel);
            try
            {
                _smtpClientWrapper.Send(smtpHost, mailMessage);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex.Message, ex);
                return new BadRequestObjectResult(new ValidationResult("Failed to send email - check SMTP settings and try again (see application log for more details)"));
            }
            return new OkResult();
        }
    }
}
