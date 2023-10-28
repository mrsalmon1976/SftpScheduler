using SftpSchedulerService.Models.Job;

namespace SftpSchedulerService.Tests.Builders.Models.JobLog
{
    public class JobLogViewModelBuilder
	{
		private JobLogViewModel _jobLogViewModel = new JobLogViewModel();

		public JobLogViewModelBuilder WithJobId(int jobId)
		{
			_jobLogViewModel.JobId = jobId;
			return this;
		}

        public JobLogViewModelBuilder WithId(int id)
        {
            _jobLogViewModel.Id = id;
            return this;
        }

        public JobLogViewModelBuilder WithRandomProperties()
        {
            _jobLogViewModel.Id = Faker.RandomNumber.Next(1, 1000);
            _jobLogViewModel.JobId = Faker.RandomNumber.Next(1, 1000);
            _jobLogViewModel.StartDate = DateTime.Now.AddSeconds(Faker.RandomNumber.Next(1, Int32.MaxValue) * -1);
            _jobLogViewModel.Progress = Faker.RandomNumber.Next(1, 100);
            _jobLogViewModel.Status = Faker.Lorem.GetFirstWord();
            return this;
        }

		public JobLogViewModel Build()
        {
            return _jobLogViewModel;
        }
    }
}