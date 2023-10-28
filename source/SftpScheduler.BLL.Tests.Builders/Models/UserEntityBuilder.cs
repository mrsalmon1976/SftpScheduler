using NSubstitute;
using SftpScheduler.BLL.Models;

namespace SftpScheduler.BLL.Tests.Builders.Models
{
    public class UserEntityBuilder
    {

		private UserEntity _userEntity = new UserEntity();

		public UserEntityBuilder WithRandomProperties()
		{
			_userEntity.Id = Guid.NewGuid().ToString();
			_userEntity.UserName = Guid.NewGuid().ToString();
			_userEntity.Email = Faker.Internet.Email();
			return this;
		}

		public UserEntityBuilder WithUserName(string userName)
		{
			_userEntity.UserName = userName;
			return this;
		}

		public UserEntity Build()
		{
			return _userEntity;
		}
	}
}
