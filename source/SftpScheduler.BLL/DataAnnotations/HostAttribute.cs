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
    public class HostAttribute : ValidationAttribute
    {
        public HostAttribute() : base("Please provide a valid url")
        {

        }

        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return true;
            }
            var hostName = value.ToString();
            if (IPAddress.TryParse(hostName, out var iPAddress))
            {
                if (hostName.Count(c => c == '.') != 3)
                {
                    return false;
                }
                return true;
            }
            if (Uri.CheckHostName(hostName) == UriHostNameType.Dns)
            {
                return true;
            }
            return false;
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
