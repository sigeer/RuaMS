using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Utility;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Application.Core.Login.ServerData
{
    public class AccountGameManager : IStorage
    {
        readonly IMapper _mapper;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly MasterServer _server;

        ConcurrentDictionary<int, IStoreUnit<Dto.AccountGameDto>> _accGameDataSource = new();

        public AccountGameManager(IMapper mapper, IDbContextFactory<DBContext> dbContextFactory, MasterServer server)
        {
            _mapper = mapper;
            _dbContextFactory = dbContextFactory;
            _server = server;
        }

        public async Task Commit(DBContext dbContext)
        {
            var updateData = _accGameDataSource.Where(x => x.Value.Flag != StoreFlag.Cached).ToDictionary();
            if (updateData.Count == 0)
                return;

            var updateAccounts = await dbContext.Accounts.Where(x => updateData.Keys.Contains(x.Id)).ToListAsync();

            foreach (var acc in updateData)
            {
                if (acc.Value.Data == null)
                    continue;

                acc.Value.Flag = StoreFlag.Cached;
                var dbAcc = updateAccounts.FirstOrDefault(x => x.Id == acc.Key);
                if (dbAcc == null)
                {
                    dbAcc = _mapper.Map<AccountEntity>(acc.Value.Data);
                    dbContext.Accounts.Add(dbAcc);
                }
                else
                {
                    _mapper.Map(acc.Value.Data, dbAcc);
                }
            }
            await dbContext.SaveChangesAsync();
        }

        public Task InitializeAsync(DBContext dbContext)
        {
            return Task.CompletedTask;
        }

        public Dto.AccountGameDto? GetAccountGameData(int accountId)
        {
            if (_accGameDataSource.TryGetValue(accountId, out var data) && data != null)
                return data.Data;

            using var dbContext = _dbContextFactory.CreateDbContext();
            var accountData = dbContext.Accounts.FirstOrDefault(x => x.Id == accountId);
            if (accountData == null)
                return null;

            data = new StoreUnit<Dto.AccountGameDto>(StoreFlag.Cached, _mapper.Map<Dto.AccountGameDto>(accountData));
            _accGameDataSource[accountId] = data;
            return data.Data;
        }

        public void UpdateAccountGame(Dto.AccountGameDto accountGame)
        {
            _accGameDataSource[accountGame.Id] = new StoreUnit<Dto.AccountGameDto>(StoreFlag.AddOrUpdate, accountGame);
        }
    }
}
