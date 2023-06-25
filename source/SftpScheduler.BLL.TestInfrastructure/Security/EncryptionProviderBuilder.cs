using NSubstitute;
using SftpScheduler.BLL.Net;
using SftpScheduler.BLL.Security;

namespace SftpScheduler.BLL.TestInfrastructure.Security
{
    public class EncryptionProviderBuilder
	{

		private IEncryptionProvider _encryptionProvider = Substitute.For<IEncryptionProvider>();

		public EncryptionProviderBuilder WithDecryptReturns(string encryptedString, string returnValue)
		{
			_encryptionProvider.Decrypt(encryptedString).Returns(returnValue);
			return this;
		}

		public EncryptionProviderBuilder WithEncryptReturns(string plainTextString, string returnValue)
		{
			_encryptionProvider.Encrypt(plainTextString).Returns(returnValue);
			return this;
		}

		public IEncryptionProvider Build()
		{
			return _encryptionProvider;
		}
	}
}
