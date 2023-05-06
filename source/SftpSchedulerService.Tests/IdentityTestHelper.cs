using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Tests
{
    internal class IdentityTestHelper
    {
        public static UserManager<UserEntity> CreateUserManagerMock()
        {
            IUserStore<UserEntity> userStore = Substitute.For<IUserStore<UserEntity>>();
            IOptions<IdentityOptions> options = Substitute.For<IOptions<IdentityOptions>>();
            IPasswordHasher<UserEntity> passwordHasher = Substitute.For<IPasswordHasher<UserEntity>>();
            IdentityErrorDescriber errorDescriber = new IdentityErrorDescriber();
            ILookupNormalizer lookupNormalizer = Substitute.For<ILookupNormalizer>();
            IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();
            ILogger<UserManager<UserEntity>> logger = Substitute.For<ILogger<UserManager<UserEntity>>>();


            return Substitute.For<UserManager<UserEntity>>(userStore, options, passwordHasher, Enumerable.Empty<IUserValidator<UserEntity>>(), Enumerable.Empty<IPasswordValidator<UserEntity>>()
                , lookupNormalizer, errorDescriber, serviceProvider, logger);

        }

        public static RoleManager<IdentityRole> CreateRoleManagerMock()
        {
            IRoleStore<IdentityRole> roleStore = Substitute.For<IRoleStore<IdentityRole>>();
            IdentityErrorDescriber errorDescriber = new IdentityErrorDescriber();
            ILookupNormalizer lookupNormalizer = Substitute.For<ILookupNormalizer>();
            ILogger<RoleManager<IdentityRole>> logger = Substitute.For<ILogger<RoleManager<IdentityRole>>>();

            //new RoleManager<IdentityRole>(roleStore, Enumerable.Empty<IRoleValidator<IdentityRole>>(), lookupNormalizer, errorDescriber, logger);
            return Substitute.For<RoleManager<IdentityRole>>(roleStore, Enumerable.Empty<IRoleValidator<IdentityRole>>(), lookupNormalizer, errorDescriber, logger);

        }

    }
}
