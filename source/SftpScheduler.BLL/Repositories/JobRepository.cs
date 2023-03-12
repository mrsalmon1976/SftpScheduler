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
            string sql = @"SELECT * FROM Job ORDER BY Name";
            return await dbContext.QueryAsync<JobEntity>(sql);
        }

        public virtual async Task<JobEntity> GetByIdAsync(IDbContext dbContext, int id)
        {
            string sql = @"SELECT * FROM Job WHERE Id = @Id";
            return await dbContext.QuerySingleAsync<JobEntity>(sql, new { Id = id });
        }


    }
}
