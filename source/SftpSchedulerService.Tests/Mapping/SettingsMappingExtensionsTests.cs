using NSubstitute;
using SftpScheduler.BLL.Config;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Config;
using SftpSchedulerService.Mapping;
using SftpSchedulerService.Models.Settings;

namespace SftpSchedulerService.Tests.Mapping
{
    [TestFixture]
    public class SettingsMappingExtensionsTests
    {
        [Test]
        public void PopulateFrom_GlobalUserSettingProvider_MapsAllProperties()
        {
            IGlobalUserSettingProvider provider = new SubstituteBuilder<IGlobalUserSettingProvider>().WithRandomProperties().Build();
            provider.DigestDays = new[] { DayOfWeek.Monday, DayOfWeek.Friday };
            GlobalSettingsViewModel viewModel = new GlobalSettingsViewModel();

            viewModel.PopulateFrom(provider);

            Assert.That(viewModel.DigestTime, Is.EqualTo(provider.DigestTime));
            Assert.That(viewModel.DigestDays, Is.EqualTo(new[] { "Monday", "Friday" }));
            Assert.That(viewModel.SmtpHost, Is.EqualTo(provider.SmtpHost));
            Assert.That(viewModel.SmtpPort, Is.EqualTo(provider.SmtpPort));
            Assert.That(viewModel.SmtpUserName, Is.EqualTo(provider.SmtpUserName));
            Assert.That(viewModel.SmtpPassword, Is.EqualTo(provider.SmtpPassword));
            Assert.That(viewModel.SmtpEnableSsl, Is.EqualTo(provider.SmtpEnableSsl));
            Assert.That(viewModel.SmtpFromName, Is.EqualTo(provider.SmtpFromName));
            Assert.That(viewModel.SmtpFromEmail, Is.EqualTo(provider.SmtpFromEmail));
        }

        [Test]
        public void PopulateFrom_StartupSettings_MapsAllProperties()
        {
            StartupSettings startupSettings = new SubstituteBuilder<StartupSettings>().WithRandomProperties().Build();
            GlobalSettingsViewModel viewModel = new GlobalSettingsViewModel();

            viewModel.PopulateFrom(startupSettings);

            Assert.That(viewModel.CertificatePath, Is.EqualTo(startupSettings.CertificatePath));
            Assert.That(viewModel.CertificatePassword, Is.EqualTo(startupSettings.CertificatePassword));
            Assert.That(viewModel.MaxConcurrentJobs, Is.EqualTo(startupSettings.MaxConcurrentJobs));
            Assert.That(viewModel.Port, Is.EqualTo(startupSettings.Port));
        }

        [Test]
        public void PopulateFrom_GlobalSettingsViewModel_MapsAllProperties()
        {
            GlobalSettingsViewModel viewModel = new SubstituteBuilder<GlobalSettingsViewModel>().WithRandomProperties().Build();
            StartupSettings settings = new StartupSettings();

            settings.PopulateFrom(viewModel);

            Assert.That(settings.CertificatePath, Is.EqualTo(viewModel.CertificatePath));
            Assert.That(settings.CertificatePassword, Is.EqualTo(viewModel.CertificatePassword));
            Assert.That(settings.MaxConcurrentJobs, Is.EqualTo(viewModel.MaxConcurrentJobs));
            Assert.That(settings.Port, Is.EqualTo(viewModel.Port));
        }
    }
}
