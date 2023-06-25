namespace SftpScheduler.BLL.Data
{
    public record GlobalUserSettingKey
    {
		public const string DigestTime = "Digest.Time";

		public const string DigestDays = "Digest.Days";

		public const string SmtpHost = "Smtp.Host";

		public const string SmtpPort = "Smtp.Port";

		public const string SmtpUserName = "Smtp.UserName";

		public const string SmtpPassword = "Smtp.Password";

		public const string SmtpEnableSsl = "Smtp.EnableSsl";

		public const string SmtpFromName = "Smtp.FromName";

		public const string SmtpFromEmail = "Smtp.FromEmail";

	}
}
