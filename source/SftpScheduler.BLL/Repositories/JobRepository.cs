using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SftpScheduler.BLL.Repositories
{
    public class JobRepository
    {
        public virtual async Task<IEnumerable<JobEntity>> GetAllAsync(IDbContext dbContext)
        {
            const string sql = @"SELECT * FROM Job ORDER BY Name";
            return await dbContext.QueryAsync<JobEntity>(sql);
        }

        public virtual async Task<JobEntity> GetByIdAsync(IDbContext dbContext, int id)
        {
            const string sql = @"SELECT * FROM Job WHERE Id = @Id";
            return await dbContext.QuerySingleAsync<JobEntity>(sql, new { Id = id });
        }

        public virtual async Task<JobEntity> GetByIdOrDefaultAsync(IDbContext dbContext, int id)
        {
            const string sql = @"SELECT * FROM Job WHERE Id = @Id";
            return await dbContext.QuerySingleOrDefaultAsync<JobEntity>(sql, new { Id = id });
        }

    }
}
