using SftpScheduler.BLL.Data;
using SftpSchedulerService.Utilities;
using System.Data.SQLite;

namespace SftpSchedulerService.BootStrapping
{
    public static class WebApplicationExtensions
    {
        public static void InitialiseDatabase(this WebApplication webApplication, IDbContextFactory dbContextFactory, ResourceUtils resourceUtils)
        {
            string dbPath = dbContextFactory.DbPath;
            if (!System.IO.File.Exists(dbPath))
            {
                string dbFileName = Path.GetFileName(dbPath);
                Directory.CreateDirectory(dbPath.Replace(dbFileName, ""));
                SQLiteConnection.CreateFile(dbPath);
            }

            using (IDbContext dbContext = dbContextFactory.GetDbContext())
            {
                string sql = resourceUtils.ReadResource("SftpSchedulerService.Resources.DbMigrations.sql");
                dbContext.ExecuteNonQuery(sql);
            }

        }
    }
}
