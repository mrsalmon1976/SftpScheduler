using HashidsNet;

namespace SftpSchedulerService.Utilities
{
    public class UrlUtils
    {
        private static Hashids _hashIds = new Hashids("SftpSchedulerService", 5);

        public static string Encode(int number)
        {
            return _hashIds.Encode(number);
        }

        public static int Decode(string hash) 
        {
            return _hashIds.DecodeSingle(hash);
        }
    }
}
