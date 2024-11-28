
namespace SftpSchedulerService.Config
{
    public class StartupSettings
    {

        public const int DefaultMaxConcurrentJobs = 2;

        public int MaxConcurrentJobs { get; set; } = DefaultMaxConcurrentJobs;

		public string CertificatePath { get; set; } = String.Empty;

		public string CertificatePassword { get; set; } = String.Empty;

		public int Port { get; set; } = 8642;

		public override bool Equals(object? obj)
		{
			StartupSettings? other = obj as StartupSettings;
			if (other == null) return false;

			return (
				this.MaxConcurrentJobs.Equals(other.MaxConcurrentJobs)
				&& this.CertificatePath.Equals(other.CertificatePath)
				&& this.CertificatePassword.Equals(other.CertificatePassword)
				&& this.Port.Equals(other.Port)
				);
		}

		public override int GetHashCode() 
		{
			HashCode hash = new();
			hash.Add(MaxConcurrentJobs);
			hash.Add(CertificatePath);
			hash.Add(CertificatePassword);
			hash.Add(Port);
			return hash.ToHashCode();
		}


	}
}
