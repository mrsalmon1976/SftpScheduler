using Castle.Core.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Tests
{
    internal class IdentityTestHelper
    {
        public static UserManager<IdentityUser> CreateUserManagerMock()
        {
            IUserStore<IdentityUser> userStore = Substitute.For<IUserStore<IdentityUser>>();
            IOptions<IdentityOptions> options = Substitute.For<IOptions<IdentityOptions>>();
            IPasswordHasher<IdentityUser> passwordHasher = Substitute.For<IPasswordHasher<IdentityUser>>();
            IdentityErrorDescriber errorDescriber = new IdentityErrorDescriber();
            ILookupNormalizer lookupNormalizer = Substitute.For<ILookupNormalizer>();
            IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();
            ILogger<UserManager<IdentityUser>> logger = Substitute.For<ILogger<UserManager<IdentityUser>>>();


            //new UserManager<IdentityUser>(userStore, options, passwordHasher, Enumerable.Empty<IUserValidator<IdentityUser>>(), Enumerable.Empty<IPasswordValidator<IdentityUser>>()
            //    , lookupNormalizer, errorDescriber, serviceProvider, logger);
            return Substitute.For<UserManager<IdentityUser>>(userStore, options, passwordHasher, Enumerable.Empty<IUserValidator<IdentityUser>>(), Enumerable.Empty<IPasswordValidator<IdentityUser>>()
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
