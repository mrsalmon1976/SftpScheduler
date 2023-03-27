using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SftpScheduler.BLL.Repositories
{
    public class JobLogRepository
    {
        public virtual async Task<IEnumerable<JobLogEntity>> GetAllByJobAsync(IDbContext dbContext, int jobId, int maxLogId, int maxRowCount)
        {
            const string sql = @"SELECT * from JobLog WHERE JobId = @JobId AND Id <= @MaxLogId ORDER BY Id DESC LIMIT @Limit";
            return await dbContext.QueryAsync<JobLogEntity>(sql, new { JobId = jobId, MaxLogId = maxLogId, Limit = maxRowCount});
        }


    }
}
