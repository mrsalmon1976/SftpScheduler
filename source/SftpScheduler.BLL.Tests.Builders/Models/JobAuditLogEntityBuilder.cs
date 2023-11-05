using NSubstitute;
using SftpScheduler.BLL.Models;

namespace SftpScheduler.BLL.Tests.Builders.Models
{
    public class JobAuditLogEntityBuilder
    {

		private JobAuditLogEntity _jobAuditLogEntity = new JobAuditLogEntity();

        public JobAuditLogEntityBuilder WithId(int id)
        {
            _jobAuditLogEntity.Id = id;
            return this;
        }

        public JobAuditLogEntityBuilder WithJobId(int jobId)
        {
            _jobAuditLogEntity.JobId = jobId;
            return this;
        }

        public JobAuditLogEntityBuilder WithPropertyName(string propertyName)
        {
            _jobAuditLogEntity.PropertyName = propertyName;
            return this;
        }

        public JobAuditLogEntityBuilder WithFromValue(string fromValue)
        {
            _jobAuditLogEntity.FromValue = fromValue;
            return this;
        }

        public JobAuditLogEntityBuilder WithToValue(string toValue)
        {
            _jobAuditLogEntity.ToValue = toValue;
            return this;
        }

        public JobAuditLogEntityBuilder WithRandomProperties()
		{
			_jobAuditLogEntity.Id = Faker.RandomNumber.Next(1, Int32.MaxValue);
			_jobAuditLogEntity.JobId = Faker.RandomNumber.Next(1, 10000);
            _jobAuditLogEntity.PropertyName = Faker.Internet.DomainName();
            _jobAuditLogEntity.FromValue = Guid.NewGuid().ToString();
            _jobAuditLogEntity.ToValue = Guid.NewGuid().ToString();
            _jobAuditLogEntity.UserName = Guid.NewGuid().ToString();
            _jobAuditLogEntity.Created = DateTime.UtcNow;
            return this;
        }

        public JobAuditLogEntity Build()
		{
			return _jobAuditLogEntity;
		}
	}
}
