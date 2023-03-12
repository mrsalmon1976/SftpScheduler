using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Identity;

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
