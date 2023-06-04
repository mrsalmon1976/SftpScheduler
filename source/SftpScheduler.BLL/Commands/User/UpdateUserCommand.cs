using Microsoft.AspNetCore.Identity;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Commands.User
{
    public interface IUpdateUserCommand
    {
        Task<UserEntity> ExecuteAsync(UserManager<UserEntity> userManager, UserEntity user, string password, IEnumerable<string> roles);

    }

    public class UpdateUserCommand : IUpdateUserCommand
    {
        private readonly IUserValidator _userValidator;

        public UpdateUserCommand(IUserValidator userValidator)
        {
            _userValidator = userValidator;
        }

        public virtual async Task<UserEntity> ExecuteAsync(UserManager<UserEntity> userManager, UserEntity user, string password, IEnumerable<string> roles)
        {
            ValidationResult validationResult = await _userValidator.Validate(userManager, user);
            if (!validationResult.IsValid)
            {
                throw new DataValidationException("User details supplied are invalid", validationResult);
            }

            UserEntity userEntity = await userManager.FindByIdAsync(user.Id);
            userEntity.Email = user.Email;
            userEntity.LockoutEnabled = user.LockoutEnabled;
            userEntity.LockoutEnd = user.LockoutEnd;
            var result = await userManager.UpdateAsync(userEntity);
            if (!result.Succeeded)
            {
                throw new DataValidationException("Unable to create user", new ValidationResult(result.Errors.Select(x => x.Description)));
            }

            // update the password if a value is supplied
            if (!String.IsNullOrEmpty(password))
            {
                string token = await userManager.GeneratePasswordResetTokenAsync(userEntity);
                IdentityResult resetPasswordResult = await userManager.ResetPasswordAsync(userEntity, token, password);
                if (!resetPasswordResult.Succeeded)
                {
                    throw new DataValidationException("Unable to reset user password", new ValidationResult(result.Errors.Select(x => x.Description)));
                }
            }


            IList<string> existingRoles = await userManager.GetRolesAsync(userEntity);

            // remove roles that have been taken away
            IEnumerable<string> rolesToRemove = existingRoles.Except(roles);
            if (rolesToRemove.Any())
            {
                IdentityResult removeRoleResult = await userManager.RemoveFromRolesAsync(userEntity, rolesToRemove);
                if (!removeRoleResult.Succeeded)
                {
                    throw new DataValidationException("Unable to remove roles from user", new ValidationResult(result.Errors.Select(x => x.Description)));
                }
            }

            // add any new roles
            IEnumerable<string> rolesToAdd = roles.Except(existingRoles);
            if (rolesToAdd.Any())
            {
                IdentityResult addRoleResult = await userManager.AddToRolesAsync(userEntity, rolesToAdd);
                if (!addRoleResult.Succeeded)
                {
                    throw new DataValidationException("Unable to remove roles from user", new ValidationResult(result.Errors.Select(x => x.Description)));
                }
            }

            return user;
        }
    }
}
