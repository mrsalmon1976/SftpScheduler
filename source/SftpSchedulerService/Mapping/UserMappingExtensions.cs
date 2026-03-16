using SftpScheduler.BLL.Identity;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models.User;

namespace SftpSchedulerService.Mapping
{
    public static class UserMappingExtensions
    {
        public static UserViewModel ToViewModel(this UserEntity userEntity, bool isAdmin = false)
        {
            bool isLockedOut = (userEntity.LockoutEnabled && userEntity.LockoutEnd.HasValue);
            return new UserViewModel
            {
                Id = userEntity.Id,
                UserName = userEntity.UserName,
                Email = userEntity.Email,
                Role = isAdmin ? UserRoles.Admin : UserRoles.User,
                IsEnabled = !isLockedOut,
            };
        }

        public static UserEntity ToEntity(this UserViewModel userViewModel)
        {
            return new UserEntity
            {
                Id = userViewModel.Id,
                UserName = userViewModel.UserName,
                Email = userViewModel.Email,
                LockoutEnabled = true,
                LockoutEnd = (userViewModel.IsEnabled ? null : DateTime.MaxValue),
            };
        }
    }
}
