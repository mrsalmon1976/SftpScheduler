using SftpSchedulerService.Models.HostAuditLog;
using SftpSchedulerService.Models.Job;

namespace SftpSchedulerService.Tests.Builders.Models.HostAuditLog
{
    public class HostAuditLogViewModelBuilder
    {
		private HostAuditLogViewModel _viewModel = new HostAuditLogViewModel();

		public HostAuditLogViewModelBuilder WithHostId(int hostId)
		{
			_viewModel.HostId = hostId;
			return this;
		}

        public HostAuditLogViewModelBuilder WithId(int id)
        {
            _viewModel.Id = id;
            return this;
        }

        public HostAuditLogViewModelBuilder WithRandomProperties()
        {
            _viewModel.Id = Faker.RandomNumber.Next(1, 1000);
            _viewModel.HostId = Faker.RandomNumber.Next(1, 1000);
            _viewModel.PropertyName = Faker.Lorem.GetFirstWord();
            _viewModel.FromValue = Guid.NewGuid().ToString();
            _viewModel.ToValue = Guid.NewGuid().ToString();
            _viewModel.Created = DateTime.Now.AddSeconds(Faker.RandomNumber.Next(1, Int32.MaxValue) * -1);
            return this;
        }

		public HostAuditLogViewModel Build()
        {
            return _viewModel;
        }
    }
}