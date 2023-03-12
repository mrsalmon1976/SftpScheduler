using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemWrapper.IO;

namespace SftpScheduler.BLL.Validators
{
    public class JobValidator
    {
        private readonly HostRepository _hostRepository;
        private readonly IDirectoryWrap _directoryWrap;

        public JobValidator(HostRepository hostRepository, IDirectoryWrap directoryWrap) 
        {
            _hostRepository = hostRepository;
            _directoryWrap = directoryWrap;
        }

        public virtual ValidationResult Validate(IDbContext dbContext, JobEntity jobEntity)
        {
            if (jobEntity == null)
            {
                throw new NullReferenceException("No job entity supplied for validation");
            }

            ValidationResult validationResult = new ValidationResult();
            var dataAnnotationValidationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            bool isValid = Validator.TryValidateObject(jobEntity, new ValidationContext(jobEntity), dataAnnotationValidationResults, true);
            if (!isValid)
            {
                IEnumerable<string> errors = dataAnnotationValidationResults.Where(x => !String.IsNullOrEmpty(x.ErrorMessage)).Select(x => x.ErrorMessage ?? "");
                validationResult.ErrorMessages.AddRange(errors);
                return validationResult;
            }

            // check that the host actually exists
            var hostEntity = _hostRepository.GetByIdAsync(dbContext, jobEntity.HostId.Value).GetAwaiter().GetResult();
            if (hostEntity == null)
            {
                validationResult.ErrorMessages.Add("Host does not exist");
            }

            // check the local path
            if (!_directoryWrap.Exists(jobEntity.LocalPath))
            {
                validationResult.ErrorMessages.Add("Local path does not exist");
            }

            return validationResult;
        }
    }
}
