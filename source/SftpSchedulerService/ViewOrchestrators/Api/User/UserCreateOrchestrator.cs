using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.User;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models.User;

namespace SftpSchedulerService.ViewOrchestrators.Api.User
{
    public interface IUserCreateOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(UserViewModel userViewModel);
    }

    public class UserCreateOrchestrator : IUserCreateOrchestrator
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly ICreateUserCommand _createUserCommand;

        public UserCreateOrchestrator(UserManager<UserEntity> userManager, ICreateUserCommand createUserCommand)
        {
            _userManager = userManager;
            _createUserCommand = createUserCommand;
        }

        public async Task<IActionResult> Execute(UserViewModel userViewModel)
        {
            UserEntity userEntity = new UserEntity()
            {
                UserName = userViewModel.UserName,
                Email = userViewModel.Email
            };
            string[] roles = { userViewModel.Role };
            try
            {
                userEntity = await _createUserCommand.ExecuteAsync(_userManager, userEntity, userViewModel.Password, roles);
            }
            catch (DataValidationException dve)
            {
                return new BadRequestObjectResult(dve.ValidationResult);
            }

            UserViewModel result = new UserViewModel();
            result.Id = userEntity.Id;
            result.UserName = userEntity.UserName;
            result.Email = userEntity.Email;
            result.Role = userViewModel.Role;
            return new OkObjectResult(result);
        }
    }
}
