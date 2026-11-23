using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Data
{
    public interface IDbContextFactory
    {
        /// <summary>
        /// Gets a context for the application database.
        /// </summary>
        IDbContext GetDefaultDbContext();

        /// <summary>
        /// Gets a context for the Quartz job store database.
        /// </summary>
        IDbContext GetQuartzDbContext();

        string DbPath { get; }
    }

    public class DbContextFactory : IDbContextFactory
    {
        public DbContextFactory(string dbPath, string quartzDbPath)
        {
            this.DbPath = dbPath;
            this.QuartzDbPath = quartzDbPath;
        }

        public string DbPath { get; private set; }

        public string QuartzDbPath { get; private set; }

        public IDbContext GetDefaultDbContext()
        {
            return new SQLiteDbContext(this.DbPath);
        }

        public IDbContext GetQuartzDbContext()
        {
            return new SQLiteDbContext(this.QuartzDbPath);
        }
    }
}
