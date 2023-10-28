using NSubstitute;
using SftpScheduler.BLL.Models;

namespace SftpScheduler.BLL.Tests.Builders.Models
{
    public class HostJobCountEntityBuilder
    {

		private HostJobCountEntity _hostJobCountEntity = new HostJobCountEntity();

        public HostJobCountEntityBuilder WithHostId(int hostId)
        {
            _hostJobCountEntity.HostId = hostId;
            return this;
        }

        public HostJobCountEntityBuilder WithJobCount(int jobCount)
        {
            _hostJobCountEntity.JobCount = jobCount;
            return this;
        }

        public HostJobCountEntityBuilder WithRandomProperties()
		{
            _hostJobCountEntity.HostId = Faker.RandomNumber.Next(1, 100);
            _hostJobCountEntity.JobCount = Faker.RandomNumber.Next(0, 20);
            return this;
        }

        public HostJobCountEntity Build()
		{
			return _hostJobCountEntity;
		}
	}
}
