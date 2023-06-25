using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using System.Security;

namespace SftpScheduler.BLL.Commands.Setting
{
    public interface IUpsertGlobalUserSettingCommand
	{
        Task Execute(IDbContext dbContext, GlobalUserSettingEntity settingEntity);
    }

    public class UpsertGlobalUserSettingCommand : IUpsertGlobalUserSettingCommand
	{

        public virtual async Task Execute(IDbContext dbContext, GlobalUserSettingEntity settingEntity)
        {
            string sql = @"INSERT INTO GlobalUserSetting(Id, SettingValue)
                VALUES (@Id, @SettingValue)
                ON CONFLICT(Id) DO UPDATE SET SettingValue = excluded.SettingValue;";
    		await dbContext.ExecuteNonQueryAsync(sql, settingEntity);


        }
    }
}
