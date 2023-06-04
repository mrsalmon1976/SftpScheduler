using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Identity;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.Models.User
{
    public class UserViewModel
    {
        public UserViewModel() 
        {
            this.Password = String.Empty;
            this.Role = UserRoles.User;
        }

        public string? Id { get; set; }

        public string? UserName { get; set; }

        public string? Email { get; set; }

        public string Password { get; set; } 

        public string Role { get; set; }

        public bool IsEnabled { get; set; }

    }
}
