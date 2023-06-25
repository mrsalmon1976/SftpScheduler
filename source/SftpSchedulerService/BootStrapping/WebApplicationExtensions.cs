using SftpScheduler.BLL.Commands.Notification;
using SftpScheduler.BLL.Config;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.Config;

namespace SftpSchedulerService.BootStrapping
{
    public static class WebApplicationExtensions
    {
        public static void InitialiseDatabase(this WebApplication webApplication, AppSettings appSettings)
        {
            var scopeFactory = webApplication.Services.GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var dbMigrator = scope.ServiceProvider.GetRequiredService<IDbMigrator>();
                dbMigrator.InitialiseQuartzDb(appSettings.DbPathQuartz);
                dbMigrator.MigrateDbChanges();

                var identityInitialiser = scope.ServiceProvider.GetRequiredService<IdentityInitialiser>();
                identityInitialiser.Seed();
            }
        }

        public static void InitialiseDigestJob(this WebApplication webApplication)
        {
            var scopeFactory = webApplication.Services.GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
				IGlobalUserSettingProvider settingsProvider = scope.ServiceProvider.GetRequiredService<IGlobalUserSettingProvider>();
				IUpsertDigestCommand upsertDigestCommand = scope.ServiceProvider.GetRequiredService<IUpsertDigestCommand>();

				upsertDigestCommand.Execute(settingsProvider.DigestDays, settingsProvider.DigestTime);
            }
        }

    }
}
