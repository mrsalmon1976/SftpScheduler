using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;

namespace SftpScheduler.BLL.Repositories
{
    public class GlobalUserSettingRepository
    {
        public virtual async Task<IEnumerable<GlobalUserSettingEntity>> GetAllAsync(IDbContext dbContext)
        {
            const string sql = @"SELECT * FROM GlobalUserSetting";
            return await dbContext.QueryAsync<GlobalUserSettingEntity>(sql);
        }

    }
}
