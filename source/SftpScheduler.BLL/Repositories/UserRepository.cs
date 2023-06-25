using Microsoft.AspNetCore.Identity;
using SftpScheduler.BLL.Models;

namespace SftpScheduler.BLL.Repositories
{
	public interface IUserRepository
	{
		Task<IEnumerable<UserEntity>> GetUsersInRoleAsync(string roleName);
	}

	public class UserRepository : IUserRepository
	{
		private readonly UserManager<UserEntity> _userManager;

		public UserRepository(UserManager<UserEntity> userManager) 
		{
			_userManager = userManager;
		}

		public virtual async Task<IEnumerable<UserEntity>> GetUsersInRoleAsync(string roleName) 
		{
			return await _userManager.GetUsersInRoleAsync(roleName);
		}
	}
}
