using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Identity;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Mapping;
using SftpSchedulerService.Models.User;

namespace SftpSchedulerService.ViewOrchestrators.Api.User
{
    public interface IUserFetchAllOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute();
    }

    public class UserFetchAllOrchestrator : IUserFetchAllOrchestrator
    {
        private readonly UserManager<UserEntity> _userManager;

        public UserFetchAllOrchestrator(UserManager<UserEntity> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Execute()
        {
            var users = _userManager.Users.OrderBy(x => x.UserName);
            var adminUsers = await _userManager.GetUsersInRoleAsync(UserRoles.Admin);

            var result = users.Select(x => x.ToViewModel(adminUsers.Contains(x)));
            return new OkObjectResult(result);
        }
    }
}
