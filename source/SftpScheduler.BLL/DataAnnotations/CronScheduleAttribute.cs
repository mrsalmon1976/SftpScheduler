using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.DataAnnotations
{
    public class CronScheduleAttribute : ValidationAttribute
    {
        public const string DefaultErrorMessage = "Cron schedule is invalid or not supported";

        public CronScheduleAttribute() : base(DefaultErrorMessage)
        {

        }

        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return false;
            }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            string schedule = value.ToString();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            if (string.IsNullOrEmpty(schedule))
            {
                return false;
            }

            try
            {
                TriggerBuilder.Create()
                  .WithIdentity($"{Guid.NewGuid()}", "Test-Validation-Group")
                  .WithCronSchedule(schedule)
                  .Build();
            }
            catch (Exception)
            {
                this.ErrorMessage = DefaultErrorMessage;
                return false;
            }

            return true;
        }

    }

}
