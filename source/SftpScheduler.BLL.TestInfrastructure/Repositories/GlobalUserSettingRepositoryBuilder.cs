using NSubstitute;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;

namespace SftpScheduler.BLL.TestInfrastructure.Repositories
{
    public class GlobalUserSettingRepositoryBuilder
    {

		private GlobalUserSettingRepository _globalUserSettingRepo = Substitute.For<GlobalUserSettingRepository>();

		public GlobalUserSettingRepositoryBuilder WithGetAllAsyncReturns(IDbContext dbContext, IEnumerable<GlobalUserSettingEntity> returnValue)
		{
			_globalUserSettingRepo.GetAllAsync(dbContext).Returns(Task.FromResult(returnValue));
			return this;
		}

		public GlobalUserSettingRepository Build()
		{
			return _globalUserSettingRepo;
		}
	}
}
