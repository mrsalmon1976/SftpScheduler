using System.Security.Cryptography;
using System.Text;

#pragma warning disable CA1416 // Validate platform compatibility
namespace SftpScheduler.BLL.Security
{
    public interface IEncryptionProvider
    {
        string Decrypt(string encryptedString);

        string Encrypt(string plainTextString);

    }

    public class EncryptionProvider : IEncryptionProvider
    {
        private readonly string _keyContainerName;
        private readonly UTF8Encoding _encoding;

        public const int KeySize = 2048;

        public EncryptionProvider(string keyContainerName)
        {
            _keyContainerName = keyContainerName;
            _encoding = new UTF8Encoding(false);
        }

        // TODO Use container urgently!
        public string Decrypt(string encryptedString)
        {
            if (String.IsNullOrEmpty(encryptedString))
            {
                return encryptedString;
            }
            else
            {
                byte[] secret = Convert.FromBase64String(encryptedString);
                using var rsa = CreateProvider();
                byte[] plain = rsa.Decrypt(secret, true);
                return _encoding.GetString(plain);
            }
        }

        public string Encrypt(string plainTextString)
        {
            if (String.IsNullOrEmpty(plainTextString))
            {
                return plainTextString;
            }
            else
            {
                byte[] plainTextData = _encoding.GetBytes(plainTextString);

                using var rsa = CreateProvider();
                byte[] encryptedData = rsa.Encrypt(plainTextData, true);
                return Convert.ToBase64String(encryptedData);
            }
        }

        private RSACryptoServiceProvider CreateProvider()
        {
            var parameters = new CspParameters()
            {
                KeyContainerName = _keyContainerName
            };

            return new RSACryptoServiceProvider(KeySize, parameters);
        }

    }
}
#pragma warning restore CA1416 // Validate platform compatibility
