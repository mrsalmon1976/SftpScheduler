using SftpScheduler.BLL.Models;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Mapping;
using SftpSchedulerService.Models.Settings;
using System.Net.Mail;

namespace SftpSchedulerService.Tests.Mapping
{
    [TestFixture]
    public class EmailTestMappingExtensionsTests
    {
        [Test]
        public void ToSmtpHost_MapsAllProperties()
        {
            var viewModel = new SubstituteBuilder<EmailTestViewModel>().WithRandomProperties().Build();

            SmtpHost result = viewModel.ToSmtpHost();

            Assert.That(result.Host, Is.EqualTo(viewModel.Host));
            Assert.That(result.Port, Is.EqualTo(viewModel.Port));
            Assert.That(result.UserName, Is.EqualTo(viewModel.UserName));
            Assert.That(result.Password, Is.EqualTo(viewModel.Password));
            Assert.That(result.EnableSsl, Is.EqualTo(viewModel.EnableSsl));
        }

        [Test]
        public void ToMailMessage_MapsAllProperties()
        {
            var viewModel = new SubstituteBuilder<EmailTestViewModel>().WithRandomProperties().Build();
            viewModel.ToAddress = "to@example.com";
            viewModel.FromAddress = "from@example.com";

            MailMessage result = viewModel.ToMailMessage();

            Assert.That(result.To[0].Address, Is.EqualTo(viewModel.ToAddress));
            Assert.That(result.From!.Address, Is.EqualTo(viewModel.FromAddress));
            Assert.That(result.Sender!.Address, Is.EqualTo(viewModel.FromAddress));
            Assert.That(result.Subject, Is.EqualTo(viewModel.Subject));
            Assert.That(result.Body, Is.EqualTo(viewModel.Body));
            Assert.That(result.IsBodyHtml, Is.True);
        }
    }
}
