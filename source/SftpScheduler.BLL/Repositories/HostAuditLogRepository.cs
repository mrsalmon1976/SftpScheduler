using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;

namespace SftpScheduler.BLL.Repositories
{
    public interface IHostAuditLogRepository 
    {
        Task<IEnumerable<HostAuditLogEntity>> GetByAllHostAsync(IDbContext dbContext, int hostId);
    }

    public class HostAuditLogRepository : IHostAuditLogRepository
    {
        public virtual async Task<IEnumerable<HostAuditLogEntity>> GetByAllHostAsync(IDbContext dbContext, int hostId)
        {
            const string sql = @"SELECT * FROM HostAuditLog WHERE HostId = @HostId ORDER BY Created";
            return await dbContext.QueryAsync<HostAuditLogEntity>(sql, new { HostId = hostId });
        }


    }
}
