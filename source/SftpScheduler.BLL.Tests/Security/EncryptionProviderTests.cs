using NUnit.Framework;
using SftpScheduler.BLL.Security;
using System.Security.Cryptography;

#pragma warning disable CA1416 // Validate platform compatibility

namespace SftpScheduler.BLL.Tests.Security
{
    [TestFixture]
    public class EncryptionProviderTests
    {
        [TestCase("Apassword123!")]
        [TestCase("ALongerPassword123!")]
        [TestCase("Amuchmuchmuchmuchmuch much much much much muuuuuch Longer Secret thingy with numbers 12345678905y543543543545544355!")]
        public void EncryptDecrypt_Integration_OutputMatchesInput(string plainText)
        {
            // setup

            // execute
            IEncryptionProvider encryptionProvider = new EncryptionProvider();
            string encryptedData = encryptionProvider.Encrypt(plainText);

            string decryptedData = encryptionProvider.Decrypt(encryptedData);

            // assert
            Assert.That(decryptedData, Is.EqualTo(plainText));
        }
    }
}
