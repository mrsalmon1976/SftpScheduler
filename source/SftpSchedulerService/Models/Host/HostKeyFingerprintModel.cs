using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.Models.Host
{
    public class HostKeyFingerprintViewModel
    {
        public HostKeyFingerprintViewModel(string algorithm, string keyFingerprint)
        {
            this.Algorithm = algorithm;
            this.KeyFingerprint = keyFingerprint;
        }

        public string Algorithm { get; set; }

        public string KeyFingerprint { get; set; }

    }
}
