using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Jobs
{
    public class JobUtils
    {
        public static string GetJobKeyName(int jobId)
        {
            return $"Job.{jobId}";
        }

        public static int GetJobIdFromKeyName(string keyName)
        {
            string[] tokens = (keyName ?? String.Empty).Split('.');
            if (tokens.Length <= 1)
            {
                throw new InvalidOperationException($"Invalid key name '{keyName}'");
            }
            return Convert.ToInt32(tokens[tokens.Length - 1]);
        }

        public static string GetTriggerKeyName(int jobId)
        {
            return $"Trigger.{jobId}";
        }
    }
}
