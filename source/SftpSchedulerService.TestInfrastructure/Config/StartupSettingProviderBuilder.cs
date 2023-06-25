using NSubstitute;
using SftpSchedulerService.Config;

namespace SftpSchedulerService.TestInfrastructure.Config
{
    public class StartupSettingProviderBuilder
    {
        private IStartupSettingProvider _startupSettingProvider = Substitute.For<IStartupSettingProvider>();

        public StartupSettingProviderBuilder WithRandomProperties()
        {
            _startupSettingProvider.FilePath.Returns("\\\\myserver\\" + Faker.Lorem.GetFirstWord());
            return this;
        }

        public StartupSettingProviderBuilder WithLoadReturns(StartupSettings startupSettings)
        {
            _startupSettingProvider.Load().Returns(startupSettings);
            return this;
		}

		public IStartupSettingProvider Build()
        {
            return _startupSettingProvider;
        }
    }
}