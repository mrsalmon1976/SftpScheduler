
namespace SftpSchedulerService.Config
{
    public class StartupSettings
    {

        public const int DefaultMaxConcurrentJobs = 2;

        public int MaxConcurrentJobs { get; set; } = DefaultMaxConcurrentJobs;

		public override bool Equals(object? obj)
		{
			StartupSettings? other = obj as StartupSettings;
			if (other == null) return false;

			return (
				this.MaxConcurrentJobs == other.MaxConcurrentJobs
				);
		}

		public override int GetHashCode() 
		{
			HashCode hash = new();
			hash.Add(MaxConcurrentJobs);
			return hash.ToHashCode();
		}


	}
}
