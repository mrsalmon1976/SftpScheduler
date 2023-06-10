using NUnit.Framework;
using SftpScheduler.BLL.Security;
using System.Security.Cryptography;

#pragma warning disable CA1416 // Validate platform compatibility

namespace SftpScheduler.BLL.Tests.Security
{
    [TestFixture]
    public class EncryptionProviderTests
    {
        private List<string> containers = new List<string>();

        [TearDown]
        public void EncryptionProviderTests_TearDown()
        {
            foreach (string containerName in containers)
            {
                var parameters = new CspParameters
                {
                    KeyContainerName = containerName
                };

                using var rsa = new RSACryptoServiceProvider(parameters)
                {
                    // Delete the key entry in the container.
                    PersistKeyInCsp = false
                };
                rsa.Clear();
            }
        }

        [TestCase("Apassword123!")]
        [TestCase("ALongerPassword123!")]
        [TestCase("Amuchmuchmuchmuchmuch much much much much muuuuuch Longer Secret thingy with numbers 12345678905y543543543545544355!")]
        public void EncryptDecrypt_Integration_OutputMatchesInput(string plainText)
        {
            // setup
            string containerName = $"SftpSchedulerTests_{Guid.NewGuid()}";
            containers.Add(containerName);

            //string plainText = Faker.Lorem.Sentence();

            // execute
            IEncryptionProvider encryptionProvider = new EncryptionProvider(containerName);
            string encryptedData = encryptionProvider.Encrypt(plainText);

            string decryptedData = encryptionProvider.Decrypt(encryptedData);

            // assert
            Assert.That(decryptedData, Is.EqualTo(plainText));
        }

        [Test]
        public void EncryptDecrypt_Integration_ContainerChangeDecryptThrowsException()
        {
            // setup
            string container1 = $"SftpSchedulerTests_{Guid.NewGuid()}";
            string container2 = $"SftpSchedulerTests_{Guid.NewGuid()}";
            containers.Add(container1);
            containers.Add(container2);

            string plainText = Faker.Lorem.Sentence();

            // execute
            IEncryptionProvider encryptionProvider1 = new EncryptionProvider(container1);
            string encryptedData = encryptionProvider1.Encrypt(plainText);

            IEncryptionProvider encryptionProvider2 = new EncryptionProvider(container2);
            try
            {
                encryptionProvider2.Decrypt(encryptedData);
                Assert.Fail("Cryptographic exception not thrown");
            }
            catch (CryptographicException)
            {
                Assert.Pass();
            }

        }

    }
}
