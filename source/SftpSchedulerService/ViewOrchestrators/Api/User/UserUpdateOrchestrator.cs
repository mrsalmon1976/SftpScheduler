using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.User;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models.Host;
using SftpSchedulerService.Models.User;

namespace SftpSchedulerService.ViewOrchestrators.Api.User
{
    public interface IUserUpdateOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(UserViewModel userViewModel);
    }

    public class UserUpdateOrchestrator : IUserUpdateOrchestrator
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly IUpdateUserCommand _updateUserCommand;

        public UserUpdateOrchestrator(UserManager<UserEntity> userManager, IUpdateUserCommand updateUserCommand)
        {
            _userManager = userManager;
            _updateUserCommand = updateUserCommand;
        }

        public async Task<IActionResult> Execute(UserViewModel userViewModel)
        {
            UserEntity userEntity = UserMapper.MapToEntity(userViewModel);
            try
            {
                userEntity = await _updateUserCommand.ExecuteAsync(_userManager, userEntity, userViewModel.Password, new string[] { userViewModel.Role });
            }
            catch (DataValidationException dve)
            {
                return new BadRequestObjectResult(dve.ValidationResult);
            }
            var result = UserMapper.MapToViewModel(userEntity);
            return new OkObjectResult(result);
        }
    }
}
