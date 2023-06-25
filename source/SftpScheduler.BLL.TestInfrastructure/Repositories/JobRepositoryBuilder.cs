using NSubstitute;
using NSubstitute.Core;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;

namespace SftpScheduler.BLL.TestInfrastructure.Repositories
{
    public class JobRepositoryBuilder
    {

		private JobRepository _jobRepo = Substitute.For<JobRepository>();

		public JobRepositoryBuilder WithGetAllFailingActiveAsyncReturns(IDbContext dbContext, IEnumerable<JobEntity> returnValue)
		{
			_jobRepo.GetAllFailingActiveAsync(dbContext).Returns(Task.FromResult(returnValue));
			return this;
		}

		public JobRepositoryBuilder WithGetAllFailedSinceAsyncReturns(IDbContext dbContext, DateTime dt, IEnumerable<JobEntity> returnValue)
		{
			_jobRepo.GetAllFailedSinceAsync(dbContext, dt).Returns(Task.FromResult(returnValue));
			return this;
		}


		public JobRepository Build()
		{
			return _jobRepo;
		}
	}
}
