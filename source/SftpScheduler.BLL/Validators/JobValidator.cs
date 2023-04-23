using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Utility.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Validators
{
    public interface IJobValidator
    {
        ValidationResult Validate(IDbContext dbContext, JobEntity jobEntity);
    }

    public class JobValidator : IJobValidator
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
            }

            // check that the host actually exists
            var hostEntity = _hostRepository.GetByIdAsync(dbContext, jobEntity.HostId).GetAwaiter().GetResult();
            if (hostEntity == null)
            {
                validationResult.ErrorMessages.Add("Host does not exist");
            }

            // check the local path
            if (!_directoryWrap.Exists(jobEntity.LocalPath))
            {
                validationResult.ErrorMessages.Add("Local path does not exist");
            }

            // if we are not deleting remote, then a remote archive path needs to be supplied
            if (!jobEntity.DeleteAfterDownload && String.IsNullOrWhiteSpace(jobEntity.RemoteArchivePath)) 
            {
                validationResult.ErrorMessages.Add("Remote archive path must be supplied if deletion after download is not selected");
            }

            // check local paths exist
            foreach (string localCopyPath in jobEntity.LocalCopyPathsAsEnumerable())
            {
                if (!_directoryWrap.Exists(localCopyPath))
                {
                    validationResult.ErrorMessages.Add($"Local copy path '{localCopyPath}' does not exist");
                }
            }

            return validationResult;
        }
    }
}
