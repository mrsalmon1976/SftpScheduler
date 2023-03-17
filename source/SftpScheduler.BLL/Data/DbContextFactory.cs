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
        IDbContext GetDbContext();

        string DbPath { get; }
    }

    public class DbContextFactory : IDbContextFactory
    {
        public DbContextFactory(string dbPath)
        {
            this.DbPath = dbPath;


        }

        public string DbPath { get; private set; }

        public IDbContext GetDbContext()
        {
            return new SQLiteDbContext(this.DbPath);
        }
    }
}
