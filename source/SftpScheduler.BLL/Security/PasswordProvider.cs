namespace SftpScheduler.BLL.Security
{
    public interface IPasswordProvider
    {
        bool CheckPassword(string password, string hash);

        string GenerateSalt();

        string HashPassword(string password, string salt);
    }

    public class PasswordProvider : IPasswordProvider
    {

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


        public string GenerateSalt()
        {
            return BCrypt.Net.BCrypt.GenerateSalt();
        }

        public string HashPassword(string password, string salt)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, salt);
        }

    }
}
