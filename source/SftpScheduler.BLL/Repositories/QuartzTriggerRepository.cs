using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;

namespace SftpScheduler.BLL.Repositories
{
    /// <summary>
    /// Reads Quartz trigger rows directly from the Quartz database. Use this for bulk reads: Quartz's
    /// IScheduler.GetTrigger makes one database round-trip per trigger.
    /// </summary>
    public class QuartzTriggerRepository
    {
        public virtual async Task<IEnumerable<QuartzTriggerEntity>> GetAllByGroupAsync(IDbContext dbContext, string schedulerName, string triggerGroup)
        {
            const string sql = @"SELECT TRIGGER_NAME AS TriggerName, NEXT_FIRE_TIME AS NextFireTimeTicks
                FROM QRTZ_TRIGGERS
                WHERE SCHED_NAME = @SchedulerName AND TRIGGER_GROUP = @TriggerGroup";
            return await dbContext.QueryAsync<QuartzTriggerEntity>(sql, new { SchedulerName = schedulerName, TriggerGroup = triggerGroup });
        }
    }
}
