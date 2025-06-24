using Application.EF;

namespace Application.Core.Login.Shared
{
    public interface IStorage
    {
        Task Commit(DBContext dbContext);
    }
}
