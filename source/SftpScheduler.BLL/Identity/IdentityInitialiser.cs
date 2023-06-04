using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SftpScheduler.BLL.Commands.User;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Identity
{
    public class IdentityInitialiser
    {
        private readonly ILogger<IdentityInitialiser> _logger;
        private readonly UserManager<UserEntity> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ICreateUserCommand _createUserCommand;
        public const string DefaultAdminUserName = "admin";
        public const string DefaultAdminUserPassword = "admin";

        public IdentityInitialiser(ILogger<IdentityInitialiser> logger, UserManager<UserEntity> userManager, RoleManager<IdentityRole> roleManager, ICreateUserCommand createUserCommand)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _createUserCommand = createUserCommand;
        }

        public virtual void Seed()
        {
            // check for roles
            CreateRoleIfNotExists(UserRoles.Admin);
            CreateRoleIfNotExists(UserRoles.User);

            var user = _userManager.FindByNameAsync(DefaultAdminUserName).GetAwaiter().GetResult();
            if (user == null)
            {
                user = new UserEntity()
                {
                    UserName = DefaultAdminUserName
                };
                _createUserCommand.ExecuteAsync(_userManager, user, DefaultAdminUserPassword, new string[] { UserRoles.Admin }).GetAwaiter().GetResult();
                _logger.LogInformation($"User {DefaultAdminUserName} created");
            }
        }

        private void CreateRoleIfNotExists(string roleName)
        {
            bool roleExists = _roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult();
            _logger.LogDebug($"Role {roleName} exists: {roleExists}");
            if (!roleExists) 
            {
                IdentityRole identityRole = new IdentityRole(roleName);
                var result = _roleManager.CreateAsync(identityRole).GetAwaiter().GetResult();
                if (!result.Succeeded)
                {
                    throw new DataValidationException("Unable to create role", new ValidationResult(result.Errors.Select(x => x.Description)));
                }
                _logger.LogInformation($"Role {roleName} created");
            }
        }
    }
}
