namespace SftpSchedulerService.Models.Settings
{
    public class EmailTestViewModel
    {
        public string Host { get; set; } = String.Empty;

        public int Port { get; set; } = 0;

        public string UserName { get; set; } = String.Empty;

        public string Password { get; set; } = String.Empty;

        public string FromAddress { get; set; } = String.Empty;

        public bool EnableSsl { get; set; } = true;

        public string ToAddress { get; set; } = String.Empty;

        public string Subject { get; set; } = String.Empty;

        public string Body { get; set; } = String.Empty;

    }
}
