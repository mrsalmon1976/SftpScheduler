using NSubstitute;
using Quartz.Impl.AdoJobStore.Common;
using SftpSchedulerService.Config;
using SftpSchedulerService.Models.Settings;
using SftpSchedulerService.Tests.Builders.Config;

namespace SftpSchedulerService.Tests.Builders.Models.Settings
{
    public class GlobalSettingsViewModelBuilder
	{
        private GlobalSettingsViewModel _globalSettingsViewModel = Substitute.For<GlobalSettingsViewModel>();

		public GlobalSettingsViewModelBuilder WithMaxConcurrentJobs(int maxConcurrentJobs)
		{
			_globalSettingsViewModel.MaxConcurrentJobs = maxConcurrentJobs;
			return this;
		}

		public GlobalSettingsViewModelBuilder WithDigestDays(DayOfWeek[] digestDays)
		{
			_globalSettingsViewModel.DigestDays = digestDays.Select(x => x.ToString()).ToArray();
			return this;
		}

		public GlobalSettingsViewModelBuilder WithDigestTime(int digestTime)
		{

			_globalSettingsViewModel.DigestTime = digestTime;
			return this;
		}

		public GlobalSettingsViewModelBuilder WithRandomProperties()
        {

			_globalSettingsViewModel.DigestTime = Faker.RandomNumber.Next(0, 23);
			_globalSettingsViewModel.DigestDays = new string[] { "Monday", "Wednesday", "Friday " };
			_globalSettingsViewModel.MaxConcurrentJobs = Faker.RandomNumber.Next(1, 10);
			_globalSettingsViewModel.SmtpHost = Faker.Lorem.GetFirstWord();
			_globalSettingsViewModel.SmtpPort = Faker.RandomNumber.Next(1, 65000);
			_globalSettingsViewModel.SmtpUserName = Faker.Lorem.GetFirstWord();
			_globalSettingsViewModel.SmtpPassword = Guid.NewGuid().ToString();
			_globalSettingsViewModel.SmtpEnableSsl = (Faker.RandomNumber.Next(1, 2) == 1);
			_globalSettingsViewModel.SmtpPasswordIgnored = (Faker.RandomNumber.Next(1, 2) == 1);
			_globalSettingsViewModel.SmtpFromName = Faker.Lorem.GetFirstWord(); ;
			_globalSettingsViewModel.SmtpFromEmail = Faker.Internet.Email();
            return this;
        }

		public GlobalSettingsViewModelBuilder WithSmtpHost(string smtpHost)
		{
			_globalSettingsViewModel.SmtpHost = smtpHost;
			return this;
		}

		public GlobalSettingsViewModelBuilder WithSmtpPort(int port)
		{
			_globalSettingsViewModel.SmtpPort = port;
			return this;
		}

		public GlobalSettingsViewModelBuilder WithSmtpEnableSsl(bool smtpEnableSsl)
		{
			_globalSettingsViewModel.SmtpEnableSsl = smtpEnableSsl;
			return this;
		}


		public GlobalSettingsViewModelBuilder WithSmtpFromEmail(string smtpFromEmail)
		{
			_globalSettingsViewModel.SmtpFromEmail = smtpFromEmail;
			return this;
		}

		public GlobalSettingsViewModelBuilder WithSmtpFromName(string smtpFromName)
		{
			_globalSettingsViewModel.SmtpFromName = smtpFromName;
			return this;
		}

		public GlobalSettingsViewModelBuilder WithSmtpUserName(string smtpPassword)
		{
			_globalSettingsViewModel.SmtpUserName = smtpPassword;
			return this;
		}

		public GlobalSettingsViewModelBuilder WithSmtpPassword(string smtpPassword)
		{
			_globalSettingsViewModel.SmtpPassword = smtpPassword;
			return this;
		}


		public GlobalSettingsViewModel Build()
        {
            return _globalSettingsViewModel;
        }
    }
}