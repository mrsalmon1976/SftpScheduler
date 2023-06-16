
namespace SftpSchedulerService.Config
{
    public class GlobalSettings
    {
        public const int DefaultMaxConcurrentJobs = 2;


        public int MaxConcurrentJobs { get; set; } = DefaultMaxConcurrentJobs;

        public GlobalSmtpSettings Smtp { get; set; } = new GlobalSmtpSettings();

    }
}
