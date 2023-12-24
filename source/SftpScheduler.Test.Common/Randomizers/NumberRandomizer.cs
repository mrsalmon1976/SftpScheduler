using System.Net;

namespace SftpScheduler.Test.Common.Randomizers
{
    public class NumberRandomizer
    {

        private static Random _random = new Random();

        public int Next()
        {
            return _random.Next();
        }

        public int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }

    }
}
