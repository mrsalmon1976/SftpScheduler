using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;

namespace SftpScheduler.BLL.Repositories
{
    public interface IJobAuditLogRepository 
    {
        Task<IEnumerable<JobAuditLogEntity>> GetByAllJobAsync(IDbContext dbContext, int jobId);
    }

    public class JobAuditLogRepository : IJobAuditLogRepository
    {
        public virtual async Task<IEnumerable<JobAuditLogEntity>> GetByAllJobAsync(IDbContext dbContext, int jobId)
        {
            const string sql = @"SELECT * FROM JobAuditLog WHERE JobId = @JobId ORDER BY Created";
            return await dbContext.QueryAsync<JobAuditLogEntity>(sql, new { JobId = jobId });
        }


    }
}
