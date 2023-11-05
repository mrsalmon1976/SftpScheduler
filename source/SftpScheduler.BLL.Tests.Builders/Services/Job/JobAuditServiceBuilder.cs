using NSubstitute;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Services.Job;

namespace SftpScheduler.BLL.Tests.Builders.Services.Job
{
    public class JobAuditServiceBuilder
    {
        private IJobAuditService _jobAuditService = Substitute.For<IJobAuditService>();

        public JobAuditServiceBuilder WithCompareHostsReturns(IDbContext dbContext, JobEntity currentJobEntity, JobEntity updatedJobEntity, string userName, IEnumerable<JobAuditLogEntity> returnValue)
        {
            _jobAuditService.CompareJobs(dbContext, currentJobEntity, updatedJobEntity, userName).Returns(Task.FromResult(returnValue));
            return this;
        }

        public IJobAuditService Build()
        {
            return _jobAuditService;
        }
    }
}
