using NSubstitute;
using SftpScheduler.BLL.Commands.Setting;

namespace SftpScheduler.BLL.TestInfrastructure.Commands.Setting
{
    public class UpsertGlobalUserSettingCommandBuilder
	{
        private IUpsertGlobalUserSettingCommand _upsertGloblUserSettingCommand = Substitute.For<IUpsertGlobalUserSettingCommand>();

        public IUpsertGlobalUserSettingCommand Build()
        {
            
            return _upsertGloblUserSettingCommand;
        }
    }
}
