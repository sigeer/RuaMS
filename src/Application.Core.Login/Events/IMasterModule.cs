using Application.EF;

namespace Application.Core.Login.Events
{
    public interface IMasterModule
    {
        int DeleteCharacterCheck(int id);
        void OnPlayerDeleted(int id);

        Task SaveChangesAsync(DBContext dbContext);
    }
}
