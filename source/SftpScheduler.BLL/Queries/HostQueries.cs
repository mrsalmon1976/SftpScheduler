using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Queries
{
    public class HostQueries
    {
        public virtual IEnumerable<HostEntity> GetAll(IDbContext dbContext)
        {
            string sql = @"SELECT * FROM Host";
            return dbContext.Query<HostEntity>(sql).ToArray();
        }

        public virtual HostEntity GetById(IDbContext dbContext, int id)
        {
            string sql = @"SELECT * FROM Host WHERE Id = @Id";
            return dbContext.Query<HostEntity>(sql, new { Id = id }).Single();
        }


    }
}
