using Microsoft.Extensions.Logging;
using SftpScheduler.Common.IO;
using System.Security.Cryptography.X509Certificates;

namespace SftpScheduler.BLL.Security
{
	public interface ICertificateProvider
	{
		X509Certificate2 LoadCertificate(string certificatePath, string certificatePassword);

	}

	public class CertificateProvider
	{
		private readonly IFileUtility _fileUtility;

		public CertificateProvider(IFileUtility fileUtility)
		{
			_fileUtility = fileUtility;
		}

		public X509Certificate2? LoadCertificate(string certificatePath, string certificatePassword)
		{
			if (!_fileUtility.Exists(certificatePath))
			{
				throw new IOException($"File {certificatePath} does not exist");
			}
			return new X509Certificate2(
				certificatePath,
				certificatePassword,
				X509KeyStorageFlags.MachineKeySet
			);

		}

	}
}
