using SftpSchedulerService.Models.Notification;

namespace SftpSchedulerService.Tests.Builders.Models.Notification
{
    public class JobNotificationViewModelBuilder
	{
		private JobNotificationViewModel _jobNotificationViewModel = new JobNotificationViewModel();

		public JobNotificationViewModelBuilder WithJobId(int jobId)
		{
            _jobNotificationViewModel.JobId = jobId;
			return this;
		}

		public JobNotificationViewModelBuilder WithRandomProperties()
        {
            _jobNotificationViewModel.JobId = Faker.RandomNumber.Next();
            _jobNotificationViewModel.NotificationType = GetRandomNotificationType();
            _jobNotificationViewModel.JobName = Guid.NewGuid().ToString();
            return this;
        }

		public JobNotificationViewModel Build()
        {
            return _jobNotificationViewModel;
        }

        private string GetRandomNotificationType()
        {
            int index = new Random().Next(1, AppConstants.NotificationType.All.Length) - 1;
            return AppConstants.NotificationType.All[index];
        }
    }
}