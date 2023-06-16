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
            model.SmtpHost = globalSetting.Smtp.Host;
            model.SmtpPort = globalSetting.Smtp.Port;
            model.SmtpUserName = globalSetting.Smtp.UserName;
            model.SmtpPassword = globalSetting.Smtp.Password;
            model.SmtpEnableSsl = globalSetting.Smtp.EnableSsl;
            model.SmtpFromName = globalSetting.Smtp.FromName;
            model.SmtpFromEmail = globalSetting.Smtp.FromEmail;
            return model;
        }

        public static GlobalSettings MapToConfig(GlobalSettingsViewModel viewModel)
        {
            GlobalSettings settings = new GlobalSettings();
            settings.MaxConcurrentJobs = viewModel.MaxConcurrentJobs;
            settings.Smtp.Host = viewModel.SmtpHost;
            settings.Smtp.Port = viewModel.SmtpPort;
            settings.Smtp.UserName = viewModel.SmtpUserName;
            settings.Smtp.Password = viewModel.SmtpPassword;
            settings.Smtp.EnableSsl = viewModel.SmtpEnableSsl;
            settings.Smtp.FromName = viewModel.SmtpFromName;
            settings.Smtp.FromEmail = viewModel.SmtpFromEmail;
            return settings;
        }

    }
}
