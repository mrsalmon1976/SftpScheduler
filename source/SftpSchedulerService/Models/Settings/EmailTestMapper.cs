using SftpScheduler.BLL.Models;
using System.Net.Mail;

namespace SftpSchedulerService.Models.Settings
{
    public class EmailTestMapper
    {
        public static SmtpHost MapToSmtpHost(EmailTestViewModel emailTestViewModel)
        {
            SmtpHost smtpHost = new SmtpHost();
            smtpHost.Host = emailTestViewModel.Host;
            smtpHost.Port = emailTestViewModel.Port;
            smtpHost.UserName = emailTestViewModel.UserName;
            smtpHost.Password = emailTestViewModel.Password;
            smtpHost.EnableSsl = emailTestViewModel.EnableSsl;
            return smtpHost;
        }

        public static MailMessage MapToMailMessage(EmailTestViewModel emailTestViewModel)
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
