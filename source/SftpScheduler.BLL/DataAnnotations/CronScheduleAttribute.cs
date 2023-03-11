using Dapper;
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
        public CronScheduleAttribute() : base("Please provide a valid cron schedule")
        {

        }

        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return false;
            }

            string schedule = value.ToString();
            if (string.IsNullOrEmpty(schedule))
            {
                return false;
            }

            try
            {
                CronExpressionDescriptor.ExpressionDescriptor.GetDescription(schedule);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        //protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        //{
        //    bool result = this.IsValid(value);
        //    if (result)
        //    {
        //        return ValidationResult.Success;
        //    }
        //    return new ValidationResult($"{value} is not a valid url");

        //}
    }

}
