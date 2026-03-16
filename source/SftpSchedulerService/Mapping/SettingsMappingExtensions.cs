using SftpScheduler.BLL.Config;
using SftpSchedulerService.Config;
using SftpSchedulerService.Models.Settings;

namespace SftpSchedulerService.Mapping
{
    public static class SettingsMappingExtensions
    {
        public static void PopulateFrom(this GlobalSettingsViewModel viewModel, IGlobalUserSettingProvider from)
        {
            viewModel.DigestDays = from.DigestDays.Select(x => x.ToString()).ToArray();
            viewModel.DigestTime = from.DigestTime;
            viewModel.SmtpHost = from.SmtpHost;
            viewModel.SmtpPort = from.SmtpPort;
            viewModel.SmtpUserName = from.SmtpUserName;
            viewModel.SmtpPassword = from.SmtpPassword;
            viewModel.SmtpEnableSsl = from.SmtpEnableSsl;
            viewModel.SmtpFromName = from.SmtpFromName;
            viewModel.SmtpFromEmail = from.SmtpFromEmail;
        }

        public static void PopulateFrom(this GlobalSettingsViewModel viewModel, StartupSettings from)
        {
            viewModel.CertificatePath = from.CertificatePath;
            viewModel.CertificatePassword = from.CertificatePassword;
            viewModel.MaxConcurrentJobs = from.MaxConcurrentJobs;
            viewModel.Port = from.Port;
        }

        public static void PopulateFrom(this StartupSettings settings, GlobalSettingsViewModel from)
        {
            settings.CertificatePath = from.CertificatePath;
            settings.CertificatePassword = from.CertificatePassword;
            settings.MaxConcurrentJobs = from.MaxConcurrentJobs;
            settings.Port = from.Port;
        }
    }
}
