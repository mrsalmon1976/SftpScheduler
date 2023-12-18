using Microsoft.EntityFrameworkCore.Internal;
using SftpScheduler.BLL.Utility;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                string sqlFileContents = _resourceUtils.ReadResource("SftpScheduler.BLL.Resources.DbMigrations.sql");
                IEnumerable<string> sqlStatements = Regex.Split(sqlFileContents, "--\\*\\*").Select(x => x.Trim()).Where(x => x.Length > 0);
                foreach (string sql in sqlStatements)
                {
                    try
                    {
                        dbContext.ExecuteNonQueryAsync(sql).GetAwaiter().GetResult();
                    }
                    catch (SQLiteException ex)
                    {
                        // SQLite doesn't allow for ALTER IF EXISTS, so we just ignore duplicate column names
                        if (ex.Message.Contains("duplicate column name"))
                        {
                            continue;
                        }
                        throw;
                    }
                }

            }
        }

        public void InitialiseQuartzDb(string dbPath)
        {
            if (!System.IO.File.Exists(dbPath))
            {
                string dbFileName = Path.GetFileName(dbPath);
                Directory.CreateDirectory(dbPath.Replace(dbFileName, ""));
                SQLiteConnection.CreateFile(dbPath);
            }

            using (var dbContext = new SQLiteDbContext(dbPath))
            {
                string sql = _resourceUtils.ReadResource("SftpScheduler.BLL.Resources.Quartz.sql");
                dbContext.ExecuteNonQueryAsync(sql).GetAwaiter().GetResult();

            }
        }

    }
}
