using NSubstitute;
using SftpSchedulerService.Config;

namespace SftpSchedulerService.TestInfrastructure.Config
{
    public class AppSettingsBuilder
    {
        private AppSettings _appSettings = Substitute.For<AppSettings>();

        public AppSettingsBuilder WithBaseDirectory(string baseDirectory)
        {
            _appSettings.BaseDirectory.Returns(baseDirectory);
            return this;
        }

        public AppSettings Build()
        {
            return _appSettings;
        }
    }
}