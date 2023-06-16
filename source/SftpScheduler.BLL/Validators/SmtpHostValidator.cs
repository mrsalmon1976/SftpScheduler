using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Validators
{
    public interface ISmtpHostValidator
    {
        ValidationResult Validate(SmtpHost smtpHost);
    }

    public class SmtpHostValidator : ISmtpHostValidator
    {
        public virtual ValidationResult Validate(SmtpHost smtpHost)
        {
            if (smtpHost == null) 
            { 
                throw new NullReferenceException("No SmtpHost supplied for validation"); 
            }

            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            bool isValid = Validator.TryValidateObject(smtpHost, new ValidationContext(smtpHost), validationResults, true);
            if (!isValid)
            {
                IEnumerable<string> errors = validationResults.Where(x => !String.IsNullOrEmpty(x.ErrorMessage)).Select(x => x.ErrorMessage ?? "");
                return new ValidationResult(errors);

            }

            return new ValidationResult();
        }
    }
}
