using SftpScheduler.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Exceptions
{
    public class DataValidationException : Exception
    {
        public DataValidationException(string? message, ValidationResult validationResult) : base(message)
        {
            ValidationResult = validationResult;
        }

        public ValidationResult ValidationResult { get; }
    }
}
