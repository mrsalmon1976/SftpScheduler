using Microsoft.AspNetCore.Identity;
using SftpScheduler.BLL.Command.User;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.Utilities;
using System.Data.SQLite;
using System.Runtime.CompilerServices;

namespace SftpSchedulerService.BootStrapping
{
    public static class WebApplicationExtensions
    {
        public static void InitialiseDatabase(this WebApplication webApplication)
        {
            var scopeFactory = webApplication.Services.GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var dbMigrator = scope.ServiceProvider.GetRequiredService<IDbMigrator>();
                dbMigrator.MigrateDbChanges();

                var identityInitialiser = scope.ServiceProvider.GetRequiredService<IdentityInitialiser>();
                identityInitialiser.Seed();
            }
        }

    }
}
