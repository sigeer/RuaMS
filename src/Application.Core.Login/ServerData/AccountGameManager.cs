using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.ServerData
{
    public class AccountGameManager : DataStorageBase<int, ProtoModel.AccountGameProto, AccountEntity>
    {
        readonly MasterServer _server;

        public AccountGameManager(IMapper mapper, IDbContextFactory<DBContext> dbContextFactory, MasterServer server, ILogger<AccountGameManager> logger)
            : base(StorageCategory.AccountGame, dbContextFactory, mapper, logger)
        {
            _server = server;
        }

        protected override int GetKey(ProtoModel.AccountGameProto model) => model.Id;

        public ProtoModel.AccountGameProto? GetAccountGameData(int accountId)
        {
            return Find(accountId);
        }

        public void UpdateAccountGame(ProtoModel.AccountGameProto accountGame)
        {
            SetDirty(accountGame);
        }
    }
}
