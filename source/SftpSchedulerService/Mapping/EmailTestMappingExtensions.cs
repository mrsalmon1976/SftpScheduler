using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models.Settings;
using System.Net.Mail;

namespace SftpSchedulerService.Mapping
{
    public static class EmailTestMappingExtensions
    {
        public static SmtpHost ToSmtpHost(this EmailTestViewModel emailTestViewModel)
        {
            return new SmtpHost
            {
                Host = emailTestViewModel.Host,
                Port = emailTestViewModel.Port,
                UserName = emailTestViewModel.UserName,
                Password = emailTestViewModel.Password,
                EnableSsl = emailTestViewModel.EnableSsl,
            };
        }

        public static MailMessage ToMailMessage(this EmailTestViewModel emailTestViewModel)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.To.Add(new MailAddress(emailTestViewModel.ToAddress));
            mailMessage.From = new MailAddress(emailTestViewModel.FromAddress);
            mailMessage.Sender = new MailAddress(emailTestViewModel.FromAddress);
            mailMessage.Subject = emailTestViewModel.Subject;
            mailMessage.Body = emailTestViewModel.Body;
            mailMessage.IsBodyHtml = true;
            return mailMessage;
        }
    }
}
