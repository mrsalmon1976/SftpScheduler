using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SftpScheduler.BLL.Queries
{
    public class JobQueries
    {
        public virtual async Task<IEnumerable<JobViewModel>> GetAllAsync(IDbContext dbContext)
        {
            string sql = @"SELECT * FROM Job";
            return await dbContext.QueryAsync<JobViewModel>(sql);
        }

        public virtual async Task<JobViewModel> GetByIdAsync(IDbContext dbContext, int id)
        {
            string sql = @"SELECT * FROM Job WHERE Id = @Id";
            return await dbContext.QuerySingleAsync<JobViewModel>(sql, new { Id = id });
        }


    }
}
