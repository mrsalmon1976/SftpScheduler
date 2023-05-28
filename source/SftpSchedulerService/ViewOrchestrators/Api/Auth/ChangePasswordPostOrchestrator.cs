using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Identity.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using SftpSchedulerService.Config;
using System.Text;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Commands.User;
using SftpSchedulerService.Models.Auth;
using SftpScheduler.BLL.Exceptions;

namespace SftpSchedulerService.ViewOrchestrators.Api.Auth
{
    public interface IChangePasswordPostOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> ExecuteAsync(ChangePasswordViewModel model, ClaimsPrincipal currentUserPrincipal);
    }

    public class ChangePasswordPostOrchestrator : IChangePasswordPostOrchestrator
    {
        private readonly ILogger<ChangePasswordPostOrchestrator> _logger;
        private readonly IChangePasswordCommand _changePasswordCommand;
        private readonly UserManager<UserEntity> _userManager;

        public ChangePasswordPostOrchestrator(ILogger<ChangePasswordPostOrchestrator> logger, IChangePasswordCommand changePasswordCommand, UserManager<UserEntity> userManager)
        {
            _logger = logger;
            _changePasswordCommand = changePasswordCommand;
            _userManager = userManager;
        }

        public async Task<IActionResult> ExecuteAsync(ChangePasswordViewModel model, ClaimsPrincipal currentUserPrincipal)
        {
            try
            {
                await _changePasswordCommand.ExecuteAsync(_userManager, currentUserPrincipal, model.CurrentPassword!, model.NewPassword!);
            }
            catch (DataValidationException dve)
            {
                return new BadRequestObjectResult(dve.ValidationResult);
            }
            return new OkResult();
        }

    }
}
