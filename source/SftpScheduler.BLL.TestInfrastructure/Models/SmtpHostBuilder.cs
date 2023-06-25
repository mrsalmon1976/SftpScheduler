using SftpScheduler.BLL.Models;

namespace SftpScheduler.BLL.TestInfrastructure.Models
{
    public class SmtpHostBuilder
    {

		private SmtpHost _smtpHost = new SmtpHost();

		public SmtpHostBuilder WithRandomProperties()
		{
			_smtpHost.Host = Faker.Lorem.GetFirstWord();
			_smtpHost.Port = Faker.RandomNumber.Next(1, 65535);
			_smtpHost.UserName = Faker.Lorem.GetFirstWord();
			_smtpHost.Password = Faker.Lorem.GetFirstWord();
			_smtpHost.EnableSsl = (Faker.RandomNumber.Next(1, 2) == 1);
			return this;
		}

		public SmtpHost Build()
		{
			return _smtpHost;
		}
	}
}
