using Application.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceTest.Infrastructure
{
    internal class EFTests : TestBase
    {
        [Test]
        public void ExecuteDeleteTest()
        {
            var _dbContextFactory = _sp.GetRequiredService<IDbContextFactory<DBContext>>();
            using var dbContext = _dbContextFactory.CreateDbContext();
            var time = DateTimeOffset.UtcNow;
            dbContext.Hwidaccounts.Where(X => X.ExpiresAt < time).ExecuteDelete();
            Assert.Pass();
        }
    }
}
