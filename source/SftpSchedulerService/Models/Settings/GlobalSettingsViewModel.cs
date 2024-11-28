namespace SftpSchedulerService.Models.Settings
{
    public class GlobalSettingsViewModel
    {
        public int DigestTime { get; set; }

        public string[] DigestDays { get; set; } = Array.Empty<string>();

        public int MaxConcurrentJobs { get; set; }

        public string CertificatePath { get; set; } = String.Empty;

        public string CertificatePassword { get; set; } = String.Empty;

        public int Port { get; set; }

        public string SmtpHost { get; set; } = String.Empty;

        public int SmtpPort { get; set; }

        public string SmtpUserName { get; set; } = String.Empty;

        public string SmtpPassword { get; set; } = String.Empty;

        public bool SmtpEnableSsl { get; set; } = true;

        public bool SmtpPasswordIgnored { get; set; } = true;

        public string SmtpFromName { get; set; } = String.Empty;

        public string SmtpFromEmail { get; set; } = String.Empty;

	}
}
