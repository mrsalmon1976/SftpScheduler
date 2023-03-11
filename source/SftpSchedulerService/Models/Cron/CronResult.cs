namespace SftpSchedulerService.Models.Cron
{
    internal class CronResult
    {
        public CronResult()
        {
        }

        public CronResult(bool isValid, string scheduleInWords)
        {
            this.IsValid = isValid;
            this.ScheduleInWords = scheduleInWords;
        }

        public bool IsValid { get; set; }

        public string? ScheduleInWords { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
