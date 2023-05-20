namespace SftpSchedulerService.Models.Update
{
    public class VersionCheckViewModel
    {
        public bool IsNewVersionAvailable { get; set; }

        public string? LatestReleaseVersionNumber { get; set; }
    }
}
