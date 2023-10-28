using SftpSchedulerService.Models.Job;

namespace SftpSchedulerService.Tests.Builders.Models.Job
{
    public class JobViewModelBuilder
	{
		private JobViewModel _jobViewModel = new JobViewModel();

		public JobViewModelBuilder WithName(string name)
		{
			_jobViewModel.Name = name;
			return this;
		}

		public JobViewModelBuilder WithRandomProperties()
        {
            _jobViewModel.Id = Faker.RandomNumber.Next();
            _jobViewModel.Name = Faker.Lorem.GetFirstWord();
            _jobViewModel.HostId = Faker.RandomNumber.Next(1, 9999);
            return this;
        }

		public JobViewModel Build()
        {
            return _jobViewModel;
        }
    }
}