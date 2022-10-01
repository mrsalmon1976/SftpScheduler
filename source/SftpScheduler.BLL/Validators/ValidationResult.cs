using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Validators
{
    public class ValidationResult
    {
        private readonly List<string> _errorMessages;

        public ValidationResult() : this(new List<string>())
        {

        }

        public ValidationResult(IEnumerable<string> errorMessages)
        {
            _errorMessages = errorMessages.ToList();
        }

        public bool IsValid => (!_errorMessages.Any());

        public List<string> ErrorMessages 
        {  
            get 
            { 
                return _errorMessages; 
            } 
        }

    }
}
