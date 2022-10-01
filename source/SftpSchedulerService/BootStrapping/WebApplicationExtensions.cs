using SftpScheduler.BLL.Data;
using SftpSchedulerService.Utilities;
using System.Data.SQLite;

namespace SftpSchedulerService.BootStrapping
{
    public static class WebApplicationExtensions
    {
        public static void InitialiseDatabase(this WebApplication webApplication, IDbMigrator dbMigrator)
        {
            dbMigrator.MigrateDbChanges();
        }
    }
}
