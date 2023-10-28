using NSubstitute;
using SftpScheduler.BLL.Net;

namespace SftpScheduler.BLL.Tests.Builders.Net
{
    public class SmtpClientWrapperBuilder
    {

		private ISmtpClientWrapper _smtpClientWrapper = Substitute.For<ISmtpClientWrapper>();


		public ISmtpClientWrapper Build()
		{
			return _smtpClientWrapper;
		}
	}
}
