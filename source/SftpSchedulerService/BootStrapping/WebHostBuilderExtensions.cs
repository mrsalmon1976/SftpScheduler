using NLog;
using SftpScheduler.BLL.Security;
using SftpScheduler.Common.IO;
using SftpSchedulerService.Config;
using System.Security.Cryptography.X509Certificates;

namespace SftpSchedulerService.BootStrapping
{
	public static class WebHostBuilderExtensions
	{
		private static Logger _logger = LogManager.GetCurrentClassLogger();

		public static void InitialiseProtocols(this IWebHostBuilder builder, StartupSettings startupSettings)
		{
			X509Certificate2? certificate = TryLoadCertificate(startupSettings.CertificatePath, startupSettings.CertificatePassword);

			builder.ConfigureKestrel(serverOptions =>
			{
				serverOptions.ConfigureHttpsDefaults(httpsOptions =>
				{
					httpsOptions.ServerCertificateSelector = (connectionContext, name) => certificate;
				});

				if (certificate == null)
				{
					serverOptions.ListenAnyIP(startupSettings.Port); // HTTP
				}
				else
				{
					serverOptions.ListenAnyIP(startupSettings.Port, listenOptions =>
					{
						listenOptions.UseHttps(); // HTTPS with certificate selector
					});
				}
			});
		}

		private static X509Certificate2? TryLoadCertificate(string certificatePath, string certificatePassword)
		{
			IFileUtility fileUtility = new FileUtility();
			try
			{
				if (!String.IsNullOrEmpty(certificatePath) && fileUtility.Exists(certificatePath))
				{
					return new CertificateProvider(fileUtility).LoadCertificate(certificatePath, certificatePassword);
				}

				_logger.Debug("No certificate configured or found at supplied location.");
				return null;
			}
			catch (Exception ex)
			{
				throw new ApplicationException($"Unable to load certificate from path '{certificatePath}'", ex);
			}
		}
	}
}
