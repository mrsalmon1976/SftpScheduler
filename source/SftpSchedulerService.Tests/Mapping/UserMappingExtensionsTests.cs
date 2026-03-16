using SftpScheduler.BLL.Identity;
using SftpScheduler.BLL.Models;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Mapping;
using SftpSchedulerService.Models.User;

namespace SftpSchedulerService.Tests.Mapping
{
    [TestFixture]
    public class UserMappingExtensionsTests
    {
        [Test]
        public void ToViewModel_MapsBaseProperties()
        {
            var userEntity = new SubstituteBuilder<UserEntity>().WithRandomProperties().Build();

            UserViewModel result = userEntity.ToViewModel();

            Assert.That(result.Id, Is.EqualTo(userEntity.Id));
            Assert.That(result.UserName, Is.EqualTo(userEntity.UserName));
            Assert.That(result.Email, Is.EqualTo(userEntity.Email));
        }

        [Test]
        public void ToViewModel_IsAdminFalse_SetsRoleToUser()
        {
            var userEntity = new SubstituteBuilder<UserEntity>().WithRandomProperties().Build();

            UserViewModel result = userEntity.ToViewModel(isAdmin: false);

            Assert.That(result.Role, Is.EqualTo(UserRoles.User));
        }

        [Test]
        public void ToViewModel_IsAdminTrue_SetsRoleToAdmin()
        {
            var userEntity = new SubstituteBuilder<UserEntity>().WithRandomProperties().Build();

            UserViewModel result = userEntity.ToViewModel(isAdmin: true);

            Assert.That(result.Role, Is.EqualTo(UserRoles.Admin));
        }

        [Test]
        public void ToViewModel_NotLockedOut_IsEnabledTrue()
        {
            var userEntity = new SubstituteBuilder<UserEntity>().WithRandomProperties().Build();
            userEntity.LockoutEnabled = true;
            userEntity.LockoutEnd = null;

            UserViewModel result = userEntity.ToViewModel();

            Assert.That(result.IsEnabled, Is.True);
        }

        [Test]
        public void ToViewModel_LockedOut_IsEnabledFalse()
        {
            var userEntity = new SubstituteBuilder<UserEntity>().WithRandomProperties().Build();
            userEntity.LockoutEnabled = true;
            userEntity.LockoutEnd = DateTime.MaxValue;

            UserViewModel result = userEntity.ToViewModel();

            Assert.That(result.IsEnabled, Is.False);
        }

        [Test]
        public void ToEntity_MapsBaseProperties()
        {
            var userViewModel = new SubstituteBuilder<UserViewModel>().WithRandomProperties().Build();

            UserEntity result = userViewModel.ToEntity();

            Assert.That(result.Id, Is.EqualTo(userViewModel.Id));
            Assert.That(result.UserName, Is.EqualTo(userViewModel.UserName));
            Assert.That(result.Email, Is.EqualTo(userViewModel.Email));
        }

        [Test]
        public void ToEntity_LockoutEnabledAlwaysTrue()
        {
            var userViewModel = new SubstituteBuilder<UserViewModel>().WithRandomProperties().Build();

            UserEntity result = userViewModel.ToEntity();

            Assert.That(result.LockoutEnabled, Is.True);
        }

        [Test]
        public void ToEntity_IsEnabledTrue_LockoutEndNull()
        {
            var userViewModel = new SubstituteBuilder<UserViewModel>().WithRandomProperties().Build();
            userViewModel.IsEnabled = true;

            UserEntity result = userViewModel.ToEntity();

            Assert.That(result.LockoutEnd, Is.Null);
        }

        [Test]
        public void ToEntity_IsEnabledFalse_LockoutEndMaxValue()
        {
            var userViewModel = new SubstituteBuilder<UserViewModel>().WithRandomProperties().Build();
            userViewModel.IsEnabled = false;

            UserEntity result = userViewModel.ToEntity();

            Assert.That(result.LockoutEnd, Is.EqualTo(new DateTimeOffset(DateTime.MaxValue)));
        }
    }
}
