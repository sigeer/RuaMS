using Application.EF;

namespace Application.Core.Login.Shared
{
    public interface IDataStorage
    {
        StorageCategory Category { get;  }
        Task InitializeAsync(DBContext dbContext);
        Task Commit(DBContext dbContext);
    }
}
