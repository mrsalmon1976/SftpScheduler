using NSubstitute;
using SftpScheduler.BLL.Utility;

namespace SftpScheduler.BLL.TestInfrastructure.Utility
{
    public class ResourceUtilsBuilder
    {

		private ResourceUtils _resourceUtils = Substitute.For<ResourceUtils>();

		public ResourceUtilsBuilder WithReadResourceReturns(string qualifiedName, string returnValue)
		{
			_resourceUtils.ReadResource(qualifiedName).Returns(returnValue);
			return this;
		}

		public ResourceUtils Build()
		{
			return _resourceUtils;
		}
	}
}
