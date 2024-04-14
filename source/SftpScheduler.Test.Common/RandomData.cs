using SftpScheduler.Test.Common.Randomizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SftpScheduler.Test.Common
{
    public static class RandomData
    {
        public static InternetRandomizer Internet { get; private set; } = new InternetRandomizer();

        public static NumberRandomizer Number{ get; private set; } = new NumberRandomizer();

        public static StringRandomizer String { get; private set; } = new StringRandomizer();
    }
}
