using Application.EF;

namespace Application.Core.Login.Shared
{
    public interface IStorage
    {
        Task InitializeAsync(DBContext dbContext);
        Task Commit(DBContext dbContext);
    }
}
