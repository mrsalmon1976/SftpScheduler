using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;

namespace SftpScheduler.BLL.Tests.Builders.Models
{
    public class JobLogEntityBuilder
    {

		private JobLogEntity _jobLogEntity = new JobLogEntity();

        public JobLogEntityBuilder WithJobId(int jobId)
        {
            _jobLogEntity.JobId = jobId;
            return this;
        }

        public JobLogEntityBuilder WithId(int id)
        {
            _jobLogEntity.Id = id;
            return this;
        }

        public JobLogEntityBuilder WithRandomProperties()
		{
            _jobLogEntity.JobId = Faker.RandomNumber.Next(1, 100);
            _jobLogEntity.StartDate = DateTime.Now.AddSeconds(-50);
            _jobLogEntity.EndDate = DateTime.Now.AddSeconds(-30);
            _jobLogEntity.Progress = Faker.RandomNumber.Next(1, 100);
            _jobLogEntity.Status = JobStatus.InProgress;
            return this;
		}

        public JobLogEntityBuilder WithStartDate(DateTime startDate)
        {
            _jobLogEntity.StartDate = startDate;
            return this;
        }

        public JobLogEntityBuilder WithStatus(string? status)
        {
            _jobLogEntity.Status = status;
            return this;
        }

        public JobLogEntity Build()
		{
			return _jobLogEntity;
		}
	}
}
