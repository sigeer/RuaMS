using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.ServerData
{
    public class AccountGameManager : DataStorageBase<int, AccountDto.AccountGameDto, AccountEntity>
    {
        readonly MasterServer _server;

        public AccountGameManager(IMapper mapper, IDbContextFactory<DBContext> dbContextFactory, MasterServer server, ILogger<AccountGameManager> logger)
            : base(StorageCategory.AccountGame, dbContextFactory, mapper, logger)
        {
            _server = server;
        }

        protected override int GetKey(AccountDto.AccountGameDto model) => model.Id;

        public AccountDto.AccountGameDto? GetAccountGameData(int accountId)
        {
            return Find(accountId);
        }

        public void UpdateAccountGame(AccountDto.AccountGameDto accountGame)
        {
            SetDirty(accountGame);
        }
    }
}
