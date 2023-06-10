using SftpScheduler.BLL.Identity;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Config;
using SftpSchedulerService.Models.Settings;

namespace SftpSchedulerService.Models.Settings
{
    public class GlobalSettingMapper
    {
        public static GlobalSettingsViewModel MapToViewModel(GlobalSettings globalSetting)
        {
            GlobalSettingsViewModel model = new GlobalSettingsViewModel();
            model.MaxConcurrentJobs = globalSetting.MaxConcurrentJobs;
            return model;
        }

        public static GlobalSettings MapToConfig(GlobalSettingsViewModel viewModel)
        {
            GlobalSettings settings = new GlobalSettings();
            settings.MaxConcurrentJobs = viewModel.MaxConcurrentJobs;
            return settings;
        }

    }
}
