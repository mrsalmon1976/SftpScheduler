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
    public interface ICreateUserCommand
    {
        Task<UserEntity> ExecuteAsync(UserManager<UserEntity> userManager, UserEntity user, string password, IEnumerable<string> roles);

    }

    public class CreateUserCommand : ICreateUserCommand
    {
        private readonly IUserValidator _userValidator;

        public CreateUserCommand(IUserValidator userValidator)
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

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new DataValidationException("Unable to create user", new ValidationResult(result.Errors.Select(x => x.Description)));
            }
            await userManager.AddToRolesAsync(user, roles);
            return user;
        }
    }
}
