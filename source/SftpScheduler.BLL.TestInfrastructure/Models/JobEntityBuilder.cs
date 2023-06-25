using NSubstitute;
using SftpScheduler.BLL.Models;

namespace SftpScheduler.BLL.TestInfrastructure.Models
{
    public class JobEntityBuilder
    {

		private JobEntity _jobEntity = Substitute.For<JobEntity>();

		public JobEntityBuilder WithRandomProperties()
		{
			_jobEntity.Id = Faker.RandomNumber.Next(1, Int32.MaxValue);
			_jobEntity.Name = Faker.Lorem.GetFirstWord();
			_jobEntity.HostId = Faker.RandomNumber.Next(1, Int32.MaxValue);
			_jobEntity.Type = Faker.RandomNumber.Next(1, 2) == 1 ? BLL.Data.JobType.Download : BLL.Data.JobType.Upload;
			_jobEntity.Schedule = "0 * 0 ? * * *";
			_jobEntity.ScheduleInWords = "Every minute";
			_jobEntity.LocalPath = "\\\\localshare\\" + Faker.Lorem.GetFirstWord();
			_jobEntity.RemotePath = "/" + Faker.Lorem.GetFirstWord();
			_jobEntity.DeleteAfterDownload = Faker.RandomNumber.Next(1, 2) == 1 ? true : false;
			return this;
		}

		public JobEntity Build()
		{
			return _jobEntity;
		}
	}
}
