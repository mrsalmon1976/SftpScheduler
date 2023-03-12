using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Utility;
using SftpScheduler.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Commands.Job
{
    public class CreateJobCommand
    {
        private readonly JobValidator _jobValidator;

        public CreateJobCommand(JobValidator jobValidator) 
        {
            _jobValidator = jobValidator;
        }

        public virtual async Task<JobEntity> ExecuteAsync(IDbContext dbContext, JobEntity jobEntity)
        {
            ValidationResult validationResult = _jobValidator.Validate(jobEntity);
            if (!validationResult.IsValid)
            {
                throw new DataValidationException("Job details supplied are invalid", validationResult);
            }

            string sql = @"INSERT INTO Job (Name, HostId, Schedule, Created) VALUES (@Name, @HostId, @Schedule, @Created)";
            await dbContext.ExecuteNonQueryAsync(sql, jobEntity);

            sql = @"select last_insert_rowid()";
            jobEntity.Id = await dbContext.ExecuteScalarAsync<int>(sql);

            return jobEntity;

        }
    }
}
