using NSubstitute;
using SftpScheduler.BLL.Models;

namespace SftpScheduler.BLL.Tests.Builders.Models
{
    public class HostEntityBuilder
    {

		private HostEntity _hostEntity = new HostEntity();

        public HostEntityBuilder WithId(int id)
        {
            _hostEntity.Id = id;
            return this;
        }

        public HostEntityBuilder WithHost(string host)
        {
            _hostEntity.Host = host;
            return this;
        }

        public HostEntityBuilder WithKeyFingerprint(string keyFingerprint)
        {
            _hostEntity.KeyFingerprint = keyFingerprint;
            return this;
        }

        public HostEntityBuilder WithName(string name)
        {
            _hostEntity.Name = name;
            return this;
        }

        public HostEntityBuilder WithPassword(string password)
        {
            _hostEntity.Password = password;
            return this;
        }

        public HostEntityBuilder WithPort(int port)
        {
            _hostEntity.Port = port;
            return this;
        }

        public HostEntityBuilder WithRandomProperties()
		{
			_hostEntity.Id = Faker.RandomNumber.Next(1, Int32.MaxValue);
            _hostEntity.Created = DateTime.UtcNow;
			_hostEntity.Name = Faker.Lorem.GetFirstWord();
            _hostEntity.Host = Faker.Internet.DomainName();
            _hostEntity.Port = Faker.RandomNumber.Next(1, 65535);
            _hostEntity.Username = Faker.Name.First();
            _hostEntity.Password = Guid.NewGuid().ToString();
            return this;
        }



        public HostEntity Build()
		{
			return _hostEntity;
		}
	}
}
