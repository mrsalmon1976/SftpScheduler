using Microsoft.AspNetCore.Identity;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Commands.User
{
    public interface IChangePasswordCommand
    {
        Task<UserEntity> ExecuteAsync(UserManager<UserEntity> userManager, ClaimsPrincipal claimsPrincipal, string currentPassword, string newPassword);
    }

    public class ChangePasswordCommand : IChangePasswordCommand
    {

        public virtual async Task<UserEntity> ExecuteAsync(UserManager<UserEntity> userManager, ClaimsPrincipal claimsPrincipal, string currentPassword, string newPassword)
        {
            UserEntity user = await userManager.GetUserAsync(claimsPrincipal);
            IdentityResult result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                throw new DataValidationException("Unable to change password", new ValidationResult(result.Errors.Select(x => x.Description)));
            }
            return user;
        }
    }
}
