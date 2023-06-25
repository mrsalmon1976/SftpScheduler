using NSubstitute;
using SftpScheduler.BLL.Commands.Notification;

namespace SftpScheduler.BLL.TestInfrastructure.Commands.Notification
{
    public class UpsertDigestCommandBuilder
    {
        private IUpsertDigestCommand _upsertDigestCommand = Substitute.For<IUpsertDigestCommand>();

        public IUpsertDigestCommand Build()
        {
            
            return _upsertDigestCommand;
        }
    }
}
