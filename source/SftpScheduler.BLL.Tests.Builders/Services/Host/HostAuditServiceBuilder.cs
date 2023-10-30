using NSubstitute;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Services.Host;

namespace SftpScheduler.BLL.Tests.Builders.Services.Host
{
    public class HostAuditServiceBuilder
    {
        private IHostAuditService _hostAuditService = Substitute.For<IHostAuditService>();

        public HostAuditServiceBuilder WithCompareHostsReturns(HostEntity currentHostEntity, HostEntity updatedHostEntity, string userName, IEnumerable<HostAuditLogEntity> returnValue)
        {
            _hostAuditService.CompareHosts(currentHostEntity, updatedHostEntity, userName).Returns(returnValue);
            return this;
        }

        public IHostAuditService Build()
        {
            return _hostAuditService;
        }
    }
}
