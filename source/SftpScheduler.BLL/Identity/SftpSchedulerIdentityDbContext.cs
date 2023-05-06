using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SftpScheduler.BLL.Models;

namespace SftpScheduler.BLL.Identity
{
    public class SftpSchedulerIdentityDbContext : IdentityDbContext<UserEntity>
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
