using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SftpScheduler.BLL.Repositories
{
    public class JobFileLogRepository
    {
        public virtual async Task<IEnumerable<JobFileLogEntity>> GetAllByJobAsync(IDbContext dbContext, int jobId, int maxLogId, int maxRowCount)
        {
            const string sql = @"SELECT * from JobFileLog WHERE JobId = @JobId AND Id <= @MaxLogId ORDER BY Id DESC LIMIT @Limit";
            return await dbContext.QueryAsync<JobFileLogEntity>(sql, new { JobId = jobId, MaxLogId = maxLogId, Limit = maxRowCount});
        }


    }
}
