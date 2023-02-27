using Microsoft.AspNetCore.Identity;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Utility;
using SftpScheduler.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Command.User
{
    public class CreateUserCommand
    {

        public virtual async Task<IdentityUser> ExecuteAsync(UserManager<IdentityUser> userManager, string userName, string password, IEnumerable<string> roles)
        {
            var user = new IdentityUser();
            user.Id = Guid.NewGuid().ToString();
            user.UserName = userName;

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
