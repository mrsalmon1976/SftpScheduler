using SftpScheduler.BLL.Identity;
using SftpSchedulerService.Models.User;

namespace SftpSchedulerService.Tests.Builders.Models.User
{
    public class UserViewModelBuilder
	{
		private UserViewModel _userViewModel = new UserViewModel();

        public UserViewModelBuilder WithId(string id)
        {
            _userViewModel.Id = id;
            return this;
        }

        public UserViewModelBuilder WithRandomProperties()
        {
            _userViewModel.Id = Guid.NewGuid().ToString();
            _userViewModel.Email = Faker.Internet.Email();
            _userViewModel.Role = UserRoles.User;
            return this;
        }

		public UserViewModel Build()
        {
            return _userViewModel;
        }
    }
}