
namespace SftpSchedulerService.Config
{
    public class GlobalSettings
    {
        public readonly DayOfWeek[] DefaultDigestDays = { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

        public const int DefaultDigestTime = 5;

        public const int DefaultMaxConcurrentJobs = 2;

        public GlobalSettings() 
        { 
            this.DigestDays = DefaultDigestDays;
        }

        public DayOfWeek[] DigestDays { get; set; }

        public int DigestTime { get; set; } = DefaultDigestTime;

        public int MaxConcurrentJobs { get; set; } = DefaultMaxConcurrentJobs;

        public GlobalSmtpSettings Smtp { get; set; } = new GlobalSmtpSettings();

    }
}
