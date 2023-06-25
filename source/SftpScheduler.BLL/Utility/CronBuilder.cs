using Quartz.Impl.Calendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Utility
{
    public class CronBuilder
    {
        public virtual string? Daily(IEnumerable<DayOfWeek> daysOfWeek, int hour)
        {
            if (hour < 0 || hour > 23)
            {
                throw new ArgumentOutOfRangeException("Hour must be between 0 and 23");
            }

            if (!daysOfWeek.Any())
            {
                return null;
            }

            string days = String.Join(',', daysOfWeek.Select(x => x.ToString().Substring(0, 3).ToUpperInvariant()));
            string result = $"0 0 {hour} ? * {days} *";
            return result;
        } 
    }
}
