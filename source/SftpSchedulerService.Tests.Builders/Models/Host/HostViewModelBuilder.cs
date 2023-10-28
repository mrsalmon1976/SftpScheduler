using SftpSchedulerService.Models.Host;

namespace SftpSchedulerService.Tests.Builders.Models.Host
{
    public class HostViewModelBuilder
	{
		private HostViewModel _hostViewModel = new HostViewModel();

		public HostViewModelBuilder WithName(string name)
		{
			_hostViewModel.Name = name;
			return this;
		}

		public HostViewModelBuilder WithRandomProperties()
        {
            _hostViewModel.Id = Faker.RandomNumber.Next();
            _hostViewModel.Name = Faker.Lorem.GetFirstWord();
            _hostViewModel.Host = Faker.Internet.DomainName();
            _hostViewModel.Port = Faker.RandomNumber.Next(1, 9999);
            _hostViewModel.UserName = Faker.Name.First();
            _hostViewModel.Password = Guid.NewGuid().ToString();
            return this;
        }

		public HostViewModel Build()
        {
            return _hostViewModel;
        }
    }
}