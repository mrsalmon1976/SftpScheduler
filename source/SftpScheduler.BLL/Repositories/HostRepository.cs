using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Queries
{
    public class HostRepository
    {
        public virtual Task<IEnumerable<HostEntity>> GetAllAsync(IDbContext dbContext)
        {
            string sql = @"SELECT * FROM Host";
            return dbContext.QueryAsync<HostEntity>(sql);
        }

        public virtual async Task<HostEntity> GetByIdAsync(IDbContext dbContext, int id)
        {
            string sql = @"SELECT * FROM Host WHERE Id = @Id";
            return await dbContext.QuerySingleAsync<HostEntity>(sql, new { Id = id });
        }


    }
}
