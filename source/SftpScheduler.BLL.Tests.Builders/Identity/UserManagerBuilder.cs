using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SftpScheduler.BLL.Models;

namespace SftpScheduler.BLL.Tests.Builders.Identity
{
    public class UserManagerBuilder
    {
        public UserManager<UserEntity> Build()
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
    }
}
