using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Data
{
    public interface IDbMigrator
    {
        void InitialiseQuartzDb(string dbPath);

        void MigrateDbChanges();
    }
}
