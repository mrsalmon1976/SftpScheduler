using SftpScheduler.BLL.Models;
using System.Net;
using System.Net.Mail;

namespace SftpScheduler.BLL.Net
{
    public interface ISmtpClientWrapper
    {
        void Send(SmtpHost smtpHost, string to, string from, string subject, string body);

        void Send(SmtpHost smtpHost, MailMessage mailMessage);
    }

    public class SmtpClientWrapper : ISmtpClientWrapper
    {
        public void Send(SmtpHost smtpHost, string to, string from, string subject, string body)
        {
            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(from),
                Sender = new MailAddress(from),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(new MailAddress(to));

            this.Send(smtpHost, mailMessage);
        }

        public void Send(SmtpHost smtpHost, MailMessage mailMessage)
        {
            using var mailClient = new SmtpClient(smtpHost.Host, smtpHost.Port);
            if (!String.IsNullOrWhiteSpace(smtpHost.UserName))
            {
                mailClient.UseDefaultCredentials = false;
                mailClient.Credentials = new NetworkCredential(smtpHost.UserName, smtpHost.Password);
            }
            mailClient.EnableSsl = smtpHost.EnableSsl;
            mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;

            mailClient.Send(mailMessage);
        }
    }
}
