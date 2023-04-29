namespace SftpSchedulerService.Models.Notification
{
    public class JobNotificationViewModel
    {
        public JobNotificationViewModel() : this(0, String.Empty, String.Empty)
        {
        }

        public JobNotificationViewModel(int jobId, string jobName, string notificationType)
        {
            this.JobId = jobId;
            this.JobName = jobName;
            this.NotificationType = notificationType;
        }

        public int JobId { get; set; }

        public string JobName { get; set; }

        public string NotificationType { get; set; }

    }
}
