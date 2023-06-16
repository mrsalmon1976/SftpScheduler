using System.Net.Mail;

namespace SftpSchedulerService.Config
{
    public record GlobalSmtpSettings
    {
        public string Host { get; set; } = String.Empty;

        public int Port { get; set; } = 25;

        public bool EnableSsl { get; set; } = true;

        public string UserName { get; set; } = String.Empty;

        public string Password { get; set; } = String.Empty;

        public string FromName { get; set; } = String.Empty;

        public string FromEmail { get; set; } = String.Empty;

    }
}
