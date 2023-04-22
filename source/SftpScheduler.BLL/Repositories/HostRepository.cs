using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Repositories
{
    public class HostRepository
    {
        public virtual async Task<IEnumerable<HostEntity>> GetAllAsync(IDbContext dbContext)
        {
            const string sql = @"SELECT * FROM Host ORDER BY Name";
            return await dbContext.QueryAsync<HostEntity>(sql);
        }

        public virtual async Task<HostEntity> GetByIdAsync(IDbContext dbContext, int id)
        {
            const string sql = @"SELECT * FROM Host WHERE Id = @Id";
            return await dbContext.QuerySingleAsync<HostEntity>(sql, new { Id = id });
        }

        public virtual async Task<HostEntity> GetByIdOrDefaultAsync(IDbContext dbContext, int id)
        {
            const string sql = @"SELECT * FROM Host WHERE Id = @Id";
            return await dbContext.QuerySingleOrDefaultAsync<HostEntity>(sql, new { Id = id });
        }


        public virtual async Task<IEnumerable<HostJobCountEntity>> GetAllJobCountsAsync(IDbContext dbContext)
        {
            const string sql = @"SELECT Host.Id AS HostId, COUNT(Job.Id) AS JobCount
                FROM HOST
                LEFT JOIN Job ON Host.Id = Job.HostId
                GROUP BY Host.Id";
            return await dbContext.QueryAsync<HostJobCountEntity>(sql);
        }

        public virtual async Task<int> GetJobCountAsync(IDbContext dbContext, int id)
        {
            const string sql = @"SELECT COUNT(Id) FROM Job WHERE HostId = @HostId";
            return await dbContext.QuerySingleAsync<int>(sql, new { HostId = id });
        }

    }
}
