using Application.EF;
using Dto;

namespace Application.Core.Login.Events
{
    public interface IMasterModule
    {
        void Initialize();
        int DeleteCharacterCheck(int id);
        void OnPlayerDeleted(int id);

        Task SaveChangesAsync(DBContext dbContext);
    }
}
