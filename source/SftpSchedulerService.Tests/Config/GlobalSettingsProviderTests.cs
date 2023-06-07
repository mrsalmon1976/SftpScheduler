using Newtonsoft.Json;
using NSubstitute;
using SftpScheduler.Common.IO;
using SftpScheduler.Common.TestInfrastructure.IO;
using SftpSchedulerService.Config;
using SftpSchedulerService.TestInfrastructure.Config;

namespace SftpSchedulerService.Tests.Config
{
    [TestFixture]
    public class GlobalSettingsProviderTests
    {
        #region GetValue Tests


        [Test]
        public void LoadAll_MaxConcurrentJobs_IsSet_ReturnsStoredValue()
        {
            // setup 
            GlobalSettings settings = new GlobalSettings();
            settings.MaxConcurrentJobs = Faker.RandomNumber.Next(3, 20);
            string json = JsonConvert.SerializeObject(settings);

            IFileUtility fileUtility = new FileUtilityBuilder()
                .WithExistsReturns(Arg.Any<string>(), true)
                .WithReadAllTextReturns(Arg.Any<string>(), json)
                .Build();

            // execute
            IGlobalSettingsProvider provider = new GlobalSettingsProvider(Substitute.For<AppSettings>(), Substitute.For<IDirectoryUtility>(), fileUtility);
            GlobalSettings result = provider.Load();

            // assert
            fileUtility.Received(1).Exists(provider.FilePath);
            fileUtility.Received(1).ReadAllText(provider.FilePath);
            Assert.That(result.MaxConcurrentJobs, Is.EqualTo(settings.MaxConcurrentJobs));
        }

        [Test]
        public void LoadAll_MaxConcurrentJobs_NotSet_ReturnsDefaultValue()
        {
            // setup 
            int defaultValue = GlobalSettings.DefaultMaxConcurrentJobs;
            GlobalSettings settings = new GlobalSettings();
            settings.MaxConcurrentJobs = Faker.RandomNumber.Next(defaultValue * 2, defaultValue * 4);

            IFileUtility fileUtility = new FileUtilityBuilder()
                .WithExistsReturns(Arg.Any<string>(), false)
                .Build();

            // execute
            IGlobalSettingsProvider provider = new GlobalSettingsProvider(Substitute.For<AppSettings>(), Substitute.For<IDirectoryUtility>(), fileUtility);
            GlobalSettings result = provider.Load();

            // assert
            fileUtility.Received(1).Exists(provider.FilePath);
            fileUtility.DidNotReceive().ReadAllText(provider.FilePath);
            Assert.That(result.MaxConcurrentJobs, Is.EqualTo(defaultValue));
        }

        #endregion

    }
}
