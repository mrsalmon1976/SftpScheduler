using NSubstitute;
using SftpScheduler.BLL.Config;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using System.Net.Mail;

namespace SftpScheduler.BLL.TestInfrastructure.Config
{
    public class GlobalUserSettingProviderBuilder
	{

		private IGlobalUserSettingProvider _provider = Substitute.For<IGlobalUserSettingProvider>();

		public GlobalUserSettingProviderBuilder WithBuildDefaultMailMessageReturns(MailMessage returnValue)
		{
			_provider.BuildDefaultMailMessage().Returns(returnValue);
			return this;
		}

		public GlobalUserSettingProviderBuilder WithBuildSmtpHostFromSettingsReturns(SmtpHost returnValue)
		{
			_provider.BuildSmtpHostFromSettings().Returns(returnValue);
			return this;
		}

		public GlobalUserSettingProviderBuilder WithDigestDays(DayOfWeek[] digestDays)
		{
			_provider.DigestDays.Returns(digestDays);
			return this;
		}

		public GlobalUserSettingProviderBuilder WithDigestTime(int port)
		{
			_provider.DigestTime.Returns(port);
			return this;
		}

		public GlobalUserSettingProviderBuilder WithSmtpHost(string smtpHost)
		{
			_provider.SmtpHost.Returns(smtpHost);
			return this;
		}

		public GlobalUserSettingProviderBuilder WithSmtpPort(int port)
		{
			_provider.SmtpPort.Returns(port);
			return this;
		}

		public GlobalUserSettingProviderBuilder WithSmtpEnableSsl(bool smtpEnableSsl)
		{
			_provider.SmtpEnableSsl.Returns(smtpEnableSsl);
			return this;
		}

		public GlobalUserSettingProviderBuilder WithSmtpFromEmail(string smtpFromEmail)
		{
			_provider.SmtpFromEmail.Returns(smtpFromEmail);
			return this;
		}

		public GlobalUserSettingProviderBuilder WithSmtpFromName(string smtpFromName)
		{
			_provider.SmtpFromName.Returns(smtpFromName);
			return this;
		}

		public GlobalUserSettingProviderBuilder WithSmtpUserName(string smtpPassword)
		{
			_provider.SmtpUserName.Returns(smtpPassword);
			return this;
		}

		public GlobalUserSettingProviderBuilder WithSmtpPassword(string smtpPassword)
		{
			_provider.SmtpPassword.Returns(smtpPassword);
			return this;
		}

		public GlobalUserSettingProviderBuilder WithRandomProperties()
		{

			_provider.DigestDays.Returns(new DayOfWeek[] { });
			_provider.DigestTime.Returns(Faker.RandomNumber.Next(0, 23));
			_provider.SmtpEnableSsl.Returns(Faker.RandomNumber.Next(1, 2) == 1);
			_provider.SmtpFromEmail.Returns(Faker.Internet.Email());
			_provider.SmtpFromName.Returns(Faker.Lorem.GetFirstWord());
			_provider.SmtpHost.Returns(Faker.Lorem.GetFirstWord());
			_provider.SmtpPassword.Returns(Faker.Lorem.GetFirstWord());
			_provider.SmtpPort.Returns(Faker.RandomNumber.Next(1, 65000));
			_provider.SmtpUserName.Returns(Faker.Lorem.GetFirstWord());
			return this;
		}

		public GlobalUserSettingProviderBuilder WithUpdateChangedValueReturns(IDbContext dbContext, string settingKey, int oldValue, int newValue, bool returnValue)
		{
			_provider.UpdateChangedValue(dbContext, settingKey, oldValue, newValue).Returns(returnValue);
			return this;
		}

		public GlobalUserSettingProviderBuilder WithUpdateChangedValueReturns(IDbContext dbContext, string settingKey, string oldValue, string newValue, bool returnValue)
		{
			_provider.UpdateChangedValue(dbContext, settingKey, oldValue, newValue).Returns(returnValue);
			return this;
		}

		public GlobalUserSettingProviderBuilder WithUpdateChangedValueReturns(IDbContext dbContext, string settingKey, string[] oldValue, string[] newValue, bool returnValue)
		{
			_provider.UpdateChangedValue(dbContext, settingKey, oldValue, newValue).Returns(returnValue);
			return this;
		}

		public IGlobalUserSettingProvider Build()
		{
			return _provider;
		}
	}
}
