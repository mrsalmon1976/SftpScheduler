using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SftpSchedulerService.BLL.Identity
{
    public class SftpSchedulerIdentityDbContext : IdentityDbContext<IdentityUser>
    {
        public SftpSchedulerIdentityDbContext(DbContextOptions<SftpSchedulerIdentityDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
