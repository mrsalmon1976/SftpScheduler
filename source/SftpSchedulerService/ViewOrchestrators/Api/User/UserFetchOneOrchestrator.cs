using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Identity;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models.User;

namespace SftpSchedulerService.ViewOrchestrators.Api.User
{
    public interface IUserFetchOneOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(string id);
    }

    public class UserFetchOneOrchestrator : IUserFetchOneOrchestrator
    {
        private readonly UserManager<UserEntity> _userManager;

        public UserFetchOneOrchestrator(UserManager<UserEntity> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Execute(string id)
        {
            UserEntity user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return new NotFoundResult();
            }
            IList<string> roles = await _userManager.GetRolesAsync(user);

            UserViewModel result = UserMapper.MapToViewModel(user, roles.Contains(UserRoles.Admin));
            return new OkObjectResult(result);
        }
    }
}
