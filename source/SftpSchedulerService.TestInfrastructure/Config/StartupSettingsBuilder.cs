using NSubstitute;
using SftpSchedulerService.Config;

namespace SftpSchedulerService.TestInfrastructure.Config
{
    public class StartupSettingsBuilder
    {
        private StartupSettings _startupSettings = Substitute.For<StartupSettings>();

		public StartupSettingsBuilder WithMaxConcurrentJobs(int maxConcurrentJobs)
		{
			_startupSettings.MaxConcurrentJobs = maxConcurrentJobs;
			return this;
		}

		public StartupSettingsBuilder WithRandomProperties()
        {
            _startupSettings.MaxConcurrentJobs = Faker.RandomNumber.Next(1, 10);
            return this;
        }

		public StartupSettings Build()
        {
            return _startupSettings;
        }
    }
}