using Microsoft.AspNetCore.Identity;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Identity;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.Common.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Validators
{
    public interface IUserValidator
    {
        Task<ValidationResult> Validate(UserManager<UserEntity> userManager, UserEntity userEntity);
    }

    public class UserValidator : IUserValidator
    {
        private readonly IEmailValidator _emailValidator;

        public UserValidator(IEmailValidator emailValidator) 
        {
            _emailValidator = emailValidator;
        }

        public virtual async Task<ValidationResult> Validate(UserManager<UserEntity> userManager, UserEntity userEntity)
        {
            if (userEntity == null)
            {
                throw new NullReferenceException("No user entity supplied for validation");
            }

            ValidationResult validationResult = new ValidationResult();

            // check that the user name is not a duplicate
            var userDuplicate = await userManager.FindByNameAsync(userEntity.UserName);
            if (userDuplicate != null && userDuplicate.Id != userEntity.Id)
            {
                validationResult.ErrorMessages.Add("User with selected user name already exists");
            }

            if (!_emailValidator.IsValidEmail(userEntity.Email))
            {
                validationResult.ErrorMessages.Add("Invalid email address supplied");
            }

            return validationResult;
        }
    }
}
