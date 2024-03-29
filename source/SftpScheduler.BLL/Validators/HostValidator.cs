﻿using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Validators
{
    public interface IHostValidator
    {
        ValidationResult Validate(HostEntity hostEntity);
    }

    public class HostValidator : IHostValidator
    {
        public virtual ValidationResult Validate(HostEntity smtpHostEntity)
        {
            if (smtpHostEntity == null) 
            { 
                throw new NullReferenceException("No host entity supplied for validation"); 
            }

            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            bool isValid = Validator.TryValidateObject(smtpHostEntity, new ValidationContext(smtpHostEntity), validationResults, true);
            if (!isValid)
            {
                IEnumerable<string> errors = validationResults.Where(x => !String.IsNullOrEmpty(x.ErrorMessage)).Select(x => x.ErrorMessage ?? "");
                return new ValidationResult(errors);

            }

            return new ValidationResult();
        }
    }
}
