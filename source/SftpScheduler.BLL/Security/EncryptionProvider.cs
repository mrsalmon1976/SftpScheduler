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

        public EncryptionProvider()
        {
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
                byte[] decryptedData = ProtectedData.Unprotect(secret, null, DataProtectionScope.LocalMachine);
                return Encoding.UTF8.GetString(decryptedData);
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
                byte[] plainTextData = Encoding.UTF8.GetBytes(plainTextString);
                byte[] encryptedData = ProtectedData.Protect(plainTextData, null, DataProtectionScope.LocalMachine);
                return Convert.ToBase64String(encryptedData);
            }
        }
    }
}
#pragma warning restore CA1416 // Validate platform compatibility
