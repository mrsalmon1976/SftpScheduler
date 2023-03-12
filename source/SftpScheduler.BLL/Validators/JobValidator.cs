using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Validators
{
    public class JobValidator
    {
        private readonly HostRepository _hostRepository;

        public JobValidator(HostRepository hostRepository) 
        {
            _hostRepository = hostRepository;
        }

        public virtual ValidationResult Validate(IDbContext dbContext, JobEntity jobEntity)
        {
            if (jobEntity == null)
            {
                throw new NullReferenceException("No job entity supplied for validation");
            }

            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            bool isValid = Validator.TryValidateObject(jobEntity, new ValidationContext(jobEntity), validationResults, true);
            if (!isValid)
            {
                IEnumerable<string> errors = validationResults.Where(x => !String.IsNullOrEmpty(x.ErrorMessage)).Select(x => x.ErrorMessage ?? "");
                return new ValidationResult(errors);
            }

            // check that the host actually exists
            var hostEntity = _hostRepository.GetByIdAsync(dbContext, jobEntity.HostId.Value).GetAwaiter().GetResult();
            if (hostEntity == null)
            {
                return new ValidationResult("Host does not exist");
            }

            return new ValidationResult();
        }
    }
}
