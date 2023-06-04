using SftpScheduler.BLL.Identity;
using SftpScheduler.BLL.Models;

namespace SftpSchedulerService.Models.User
{
    public class UserMapper
    {
        public static UserViewModel MapToViewModel(UserEntity userEntity, bool isAdmin = false)
        {
            bool isLockedOut = (userEntity.LockoutEnabled && userEntity.LockoutEnd.HasValue);
            UserViewModel model = new UserViewModel();
            model.Id = userEntity.Id;
            model.UserName = userEntity.UserName;
            model.Email = userEntity.Email;
            model.Role = isAdmin ? UserRoles.Admin : UserRoles.User;
            model.IsEnabled = !isLockedOut;
            return model;
        }

        public static UserEntity MapToEntity(UserViewModel userViewModel)
        {
            UserEntity entity = new UserEntity();
            entity.Id = userViewModel.Id;
            entity.UserName = userViewModel.UserName;
            entity.Email = userViewModel.Email;
            entity.LockoutEnabled = true;
            entity.LockoutEnd = (userViewModel.IsEnabled ? null : DateTime.MaxValue);
            return entity;
        }

    }
}
