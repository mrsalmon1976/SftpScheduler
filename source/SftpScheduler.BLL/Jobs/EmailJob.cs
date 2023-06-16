using Microsoft.Extensions.Logging;
using Quartz;


namespace SftpScheduler.BLL.Jobs
{
    internal class EmailJob : IJob
    {
        public const string DefaultGroup = "EmailJob";

        private readonly ILogger<EmailJob> _logger;

        public EmailJob(ILogger<EmailJob> logger)
        {
            _logger = logger;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            //string sender = "TODO";
            //string recipient = "TODO";

            //MailMessage mailMessage = new MailMessage
            //{
            //    From = new MailAddress("TODO"),
            //    Sender = new MailAddress(sender),
            //    Subject = "TODO",
            //    Body = "TODO",
            //    IsBodyHtml = true
            //};
            //mailMessage.To.Add(new MailAddress("TODO"));


            //using var mailClient = new SmtpClient("TODO", 0);
            //mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            //mailClient.UseDefaultCredentials = false;

            //_logger.LogInformation("Sending message from {sender} to {recipient}; host {host}:{port}", sender, recipient, mailClient.Host, mailClient.Port);

            //mailClient.Send(mailMessage);

            //context.Result = true;
            throw new NotImplementedException();

        }
    }
}
