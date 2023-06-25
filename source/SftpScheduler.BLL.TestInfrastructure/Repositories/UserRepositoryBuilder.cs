using NSubstitute;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;

namespace SftpScheduler.BLL.TestInfrastructure.Repositories
{
    public class UserRepositoryBuilder
    {

		private IUserRepository _userRepo = Substitute.For<IUserRepository>();

		public UserRepositoryBuilder WithGetUsersInRoleAsyncReturns(string roleName, IEnumerable<UserEntity> returnValue)
		{
			_userRepo.GetUsersInRoleAsync(roleName).Returns(Task.FromResult(returnValue));
			return this;
		}


		public IUserRepository Build()
		{
			return _userRepo;
		}
	}
}
