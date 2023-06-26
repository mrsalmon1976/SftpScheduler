using NSubstitute;
using SftpScheduler.Common.IO;

namespace SftpScheduler.Common.TestInfrastructure.IO
{
    public class DirectoryUtilityBuilder
    {
        private IDirectoryUtility _dirUtility = Substitute.For<IDirectoryUtility>();

        public IDirectoryUtility Build()
        {
            return _dirUtility;
        }

        public DirectoryUtilityBuilder WithExistsReturns(string path, bool result)
        {
            _dirUtility.Exists(path).Returns(result);
            return this;
        }

        public DirectoryUtilityBuilder WithGetFilesReturns(string sourceDirectory, SearchOption searchOption, string searchPattern, string[] returnValue)
        {
            _dirUtility.GetFiles(sourceDirectory, searchOption, searchPattern).Returns(returnValue);
			return this;
        }

    }
}
