using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.Models.User;

namespace SftpSchedulerService.ViewOrchestrators.Api.User
{
    public interface IUserFetchAllOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute();
    }

    public class UserFetchAllOrchestrator : IUserFetchAllOrchestrator
    {
        private readonly IMapper _mapper;
        private readonly UserManager<UserEntity> _userManager;

        public UserFetchAllOrchestrator(IMapper mapper, UserManager<UserEntity> userManager)
        {
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<IActionResult> Execute()
        {
            var users = await Task.Run(() => _userManager.Users.OrderBy(x => x.UserName));
            var result = _mapper.Map<UserEntity[], UserViewModel[]>(users.ToArray());
            return new OkObjectResult(result);
        }
    }
}
