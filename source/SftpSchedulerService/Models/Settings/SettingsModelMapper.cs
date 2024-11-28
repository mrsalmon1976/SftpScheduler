using SftpScheduler.BLL.Config;
using SftpSchedulerService.Config;

namespace SftpSchedulerService.Models.Settings
{
    public class SettingsModelMapper
    {
        public static void MapValues(IGlobalUserSettingProvider from, GlobalSettingsViewModel to)
        {
            to.DigestDays = from.DigestDays.Select(x => x.ToString()).ToArray();
            to.DigestTime = from.DigestTime;
            to.SmtpHost = from.SmtpHost;
            to.SmtpPort = from.SmtpPort;
            to.SmtpUserName = from.SmtpUserName;
            to.SmtpPassword = from.SmtpPassword;
            to.SmtpEnableSsl = from.SmtpEnableSsl;
            to.SmtpFromName = from.SmtpFromName;
            to.SmtpFromEmail = from.SmtpFromEmail;
        }

        public static void MapValues(StartupSettings from, GlobalSettingsViewModel to)
        {
            to.CertificatePath = from.CertificatePath;
            to.CertificatePassword = from.CertificatePassword;
            to.MaxConcurrentJobs = from.MaxConcurrentJobs;
            to.Port = from.Port;
        }

        public static void MapValues(GlobalSettingsViewModel from, StartupSettings to)
        {
            to.CertificatePath = from.CertificatePath;
            to.CertificatePassword = from.CertificatePassword;
            to.MaxConcurrentJobs = from.MaxConcurrentJobs;
            to.Port = from.Port;
        }
    }
}
