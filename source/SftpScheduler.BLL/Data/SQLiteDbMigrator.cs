using Microsoft.EntityFrameworkCore.Internal;
using SftpScheduler.BLL.Utility;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Data
{
    public class SQLiteDbMigrator : IDbMigrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ResourceUtils _resourceUtils;

        public SQLiteDbMigrator(IDbContextFactory dbContextFactory, ResourceUtils resourceUtils)
        {
            this._dbContextFactory = dbContextFactory;
            this._resourceUtils = resourceUtils;
        }

        public void MigrateDbChanges()
        {
            string dbPath = _dbContextFactory.DbPath;
            if (!System.IO.File.Exists(dbPath))
            {
                string dbFileName = Path.GetFileName(dbPath);
                Directory.CreateDirectory(dbPath.Replace(dbFileName, ""));
                SQLiteConnection.CreateFile(dbPath);
            }

            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                string sql = _resourceUtils.ReadResource("SftpScheduler.BLL.Resources.DbMigrations.sql");
                dbContext.ExecuteNonQueryAsync(sql).GetAwaiter().GetResult();
            }
        }
    }
}
