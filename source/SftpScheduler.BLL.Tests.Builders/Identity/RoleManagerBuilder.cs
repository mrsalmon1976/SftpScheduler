using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace SftpScheduler.BLL.Tests.Builders.Identity
{
    public class RoleManagerBuilder
    {
        public RoleManager<IdentityRole> Build()
        {
            IRoleStore<IdentityRole> roleStore = Substitute.For<IRoleStore<IdentityRole>>();
            IdentityErrorDescriber errorDescriber = new IdentityErrorDescriber();
            ILookupNormalizer lookupNormalizer = Substitute.For<ILookupNormalizer>();
            ILogger<RoleManager<IdentityRole>> logger = Substitute.For<ILogger<RoleManager<IdentityRole>>>();

            return Substitute.For<RoleManager<IdentityRole>>(roleStore, Enumerable.Empty<IRoleValidator<IdentityRole>>(), lookupNormalizer, errorDescriber, logger);

        }
    }
}
