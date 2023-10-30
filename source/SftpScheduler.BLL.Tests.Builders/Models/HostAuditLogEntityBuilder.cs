using NSubstitute;
using SftpScheduler.BLL.Models;

namespace SftpScheduler.BLL.Tests.Builders.Models
{
    public class HostAuditLogEntityBuilder
    {

		private HostAuditLogEntity _hostAuditLogEntity = new HostAuditLogEntity();

        public HostAuditLogEntityBuilder WithId(int id)
        {
            _hostAuditLogEntity.Id = id;
            return this;
        }

        public HostAuditLogEntityBuilder WithHostId(int hostId)
        {
            _hostAuditLogEntity.HostId = hostId;
            return this;
        }

        public HostAuditLogEntityBuilder WithPropertyName(string propertyName)
        {
            _hostAuditLogEntity.PropertyName = propertyName;
            return this;
        }

        public HostAuditLogEntityBuilder WithFromValue(string fromValue)
        {
            _hostAuditLogEntity.FromValue = fromValue;
            return this;
        }

        public HostAuditLogEntityBuilder WithToValue(string toValue)
        {
            _hostAuditLogEntity.ToValue = toValue;
            return this;
        }

        public HostAuditLogEntityBuilder WithRandomProperties()
		{
			_hostAuditLogEntity.Id = Faker.RandomNumber.Next(1, Int32.MaxValue);
			_hostAuditLogEntity.HostId = Faker.RandomNumber.Next(1, 10000);
            _hostAuditLogEntity.PropertyName = Faker.Internet.DomainName();
            _hostAuditLogEntity.FromValue = Guid.NewGuid().ToString();
            _hostAuditLogEntity.ToValue = Guid.NewGuid().ToString();
            _hostAuditLogEntity.UserName = Guid.NewGuid().ToString();
            _hostAuditLogEntity.Created = DateTime.UtcNow;
            return this;
        }

        public HostAuditLogEntity Build()
		{
			return _hostAuditLogEntity;
		}
	}
}
