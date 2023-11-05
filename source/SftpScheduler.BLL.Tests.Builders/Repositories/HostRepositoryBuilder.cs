using NSubstitute;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;

namespace SftpScheduler.BLL.Tests.Builders.Repositories
{
    public class HostRepositoryBuilder
    {

		private HostRepository _hostRepo = Substitute.For<HostRepository>();

		public HostRepositoryBuilder WithGetUsersInRoleAsyncReturns(IDbContext dbContext, int hostId, HostEntity returnValue)
		{
			_hostRepo.GetByIdAsync(dbContext, hostId).Returns(Task.FromResult(returnValue));
			return this;
		}


		public HostRepository Build()
		{
			return _hostRepo;
		}
	}
}
