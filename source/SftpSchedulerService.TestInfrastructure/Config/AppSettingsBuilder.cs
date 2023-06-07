using NSubstitute;
using SftpSchedulerService.Config;

namespace SftpSchedulerService.TestInfrastructure.Config
{
    public class AppSettingsBuilder
    {
        private AppSettings _appSettings = Substitute.For<AppSettings>();

        public AppSettings Build()
        {
            return _appSettings;
        }
    }
}