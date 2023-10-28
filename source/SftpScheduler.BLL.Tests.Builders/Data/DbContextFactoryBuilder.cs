using NSubstitute;
using SftpScheduler.BLL.Data;

namespace SftpScheduler.BLL.Tests.Builders.Data
{
    public class DbContextFactoryBuilder
    {
        private IDbContextFactory _dbContextFactory = Substitute.For<IDbContextFactory>();

        public DbContextFactoryBuilder WithDbContext(IDbContext dbContext)
        {
            _dbContextFactory.GetDbContext().Returns(dbContext);
            return this;
        }


        public IDbContextFactory Build()
        {
            
            return _dbContextFactory;
        }
    }
}
