using SftpScheduler.BLL.Models;

namespace SftpScheduler.BLL.Tests.Builders.Models
{
    public class JobFileLogEntityBuilder
    {

		private JobFileLogEntity _jobFileLogEntity = new JobFileLogEntity();

        public JobFileLogEntityBuilder WithJobId(int jobId)
        {
            _jobFileLogEntity.JobId = jobId;
            return this;
        }

        public JobFileLogEntityBuilder WithRandomProperties()
		{
            _jobFileLogEntity.JobId = Faker.RandomNumber.Next(1, 100);
            _jobFileLogEntity.FileName = Faker.Name.First();
            _jobFileLogEntity.FileLength = Faker.RandomNumber.Next(1, 1000);
            _jobFileLogEntity.StartDate = DateTime.Now.AddSeconds(-50);
            _jobFileLogEntity.EndDate = DateTime.Now.AddSeconds(-30);
            return this;
		}

        public JobFileLogEntityBuilder WithStartDate(DateTime startDate)
        {
            _jobFileLogEntity.StartDate = startDate;
            return this;
        }

        public JobFileLogEntity Build()
		{
			return _jobFileLogEntity;
		}
	}
}
