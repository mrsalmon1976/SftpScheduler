﻿using System.Security.Cryptography;
using System.Text;

namespace SftpScheduler.BLL.Security
{
    public interface IPasswordProvider
    {
        bool CheckPassword(string password, string hash);

        string Decrypt(string encryptedString);

        string Encrypt(string plainTextString);

        string GenerateSalt();

        string HashPassword(string password, string salt);
    }

    public class PasswordProvider : IPasswordProvider
    {

        public PasswordProvider(string secretKey)
        {
            this.SecretKey = secretKey;
        }

        public string SecretKey { get; set; }

        public bool CheckPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string Decrypt(string encryptedString)
        {
            if (String.IsNullOrEmpty(encryptedString))
            {
                return encryptedString;
            }
            else
            {
                byte[] secret = Convert.FromBase64String(encryptedString);
#pragma warning disable CA1416 // Validate platform compatibility
                byte[] plain = ProtectedData.Unprotect(secret, this.GetSecretKeyAsBytes(), DataProtectionScope.LocalMachine);
#pragma warning restore CA1416 // Validate platform compatibility
                var encoding = new UTF8Encoding();
                return encoding.GetString(plain);
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
                var encoding = new UTF8Encoding();
                byte[] plain = encoding.GetBytes(plainTextString);
#pragma warning disable CA1416 // Validate platform compatibility
                byte[] secret = ProtectedData.Protect(plain, this.GetSecretKeyAsBytes(), DataProtectionScope.LocalMachine);
#pragma warning restore CA1416 // Validate platform compatibility
                return Convert.ToBase64String(secret);
            }
        }

        public string GenerateSalt()
        {
            return BCrypt.Net.BCrypt.GenerateSalt();
        }

        public string HashPassword(string password, string salt)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, salt);
        }

        private byte[]? GetSecretKeyAsBytes()
        {
            if (String.IsNullOrEmpty(this.SecretKey))
            {
                return null;
            }
            return Encoding.ASCII.GetBytes(this.SecretKey);
        }
    }
}
