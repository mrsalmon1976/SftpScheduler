using NSubstitute;
using SftpScheduler.BLL.Data;

namespace SftpScheduler.BLL.Tests.Builders.Data
{
    public class DbContextBuilder
    {
      
        public IDbContext Build()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            return dbContext;
        }
    }
}
