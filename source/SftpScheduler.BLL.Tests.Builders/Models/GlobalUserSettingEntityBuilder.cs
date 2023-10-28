using NSubstitute;
using SftpScheduler.BLL.Models;

namespace SftpScheduler.BLL.Tests.Builders.Models
{
    public class GlobalUserSettingEntityBuilder
    {

		private GlobalUserSettingEntity _globalUserSettingEntity = new GlobalUserSettingEntity();

        public GlobalUserSettingEntityBuilder WithId(string id)
        {
            _globalUserSettingEntity.Id = id;
            return this;
        }

        public GlobalUserSettingEntityBuilder WithSettingValue(string settingValue)
        {
            _globalUserSettingEntity.SettingValue = settingValue;
            return this;
        }

        public GlobalUserSettingEntityBuilder WithRandomProperties()
		{
            _globalUserSettingEntity.Id = Guid.NewGuid().ToString();
            _globalUserSettingEntity.SettingValue = Guid.NewGuid().ToString();
            return this;
        }



        public GlobalUserSettingEntity Build()
		{
			return _globalUserSettingEntity;
		}
	}
}
