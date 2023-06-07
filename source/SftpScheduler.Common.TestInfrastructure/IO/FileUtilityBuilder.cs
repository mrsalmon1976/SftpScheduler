using NSubstitute;
using SftpScheduler.Common.IO;

namespace SftpScheduler.Common.TestInfrastructure.IO
{
    public class FileUtilityBuilder
    {
        private IFileUtility _fileUtility = Substitute.For<IFileUtility>();

        public IFileUtility Build()
        {
            return _fileUtility;
        }

        public FileUtilityBuilder WithExistsReturns(string path, bool result)
        {
            _fileUtility.Exists(path).Returns(result);
            return this;
        }

        public FileUtilityBuilder WithReadAllTextReturns(string path, string resultText)
        {
            _fileUtility.ReadAllText(path).Returns(resultText);
            return this;
        }
    }
}
