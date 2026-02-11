using Application.Core.Login.Datas;
using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Items;
using Application.Utility;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Application.Core.Login.ServerData
{
    public class AccountGameManager : IStorage
    {
        readonly IMapper _mapper;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly MasterServer _server;

        ConcurrentDictionary<int, AccountGame> _accGameDataSource = new();
        ConcurrentDictionary<int, StoreFlag> _updated = new();

        public AccountGameManager(IMapper mapper, IDbContextFactory<DBContext> dbContextFactory, MasterServer server)
        {
            _mapper = mapper;
            _dbContextFactory = dbContextFactory;
            _server = server;
        }

        public async Task Commit(DBContext dbContext)
        {
            var updatedItems = _updated.Keys.ToList();
            _updated.Clear();
            if (updatedItems.Count == 0)
                return;

            var allItems = _accGameDataSource.Where(x => updatedItems.Contains(x.Key)).ToList();

            await dbContext.Quickslotkeymappeds.Where(x => updatedItems.Contains(x.Accountid)).ExecuteDeleteAsync();
            await dbContext.Storages.Where(x => updatedItems.Contains(x.OwnerId) && x.Type == (int)StorageType.AccountStorage).ExecuteDeleteAsync();

            foreach (var acc in allItems)
            {
                if (acc.Value.QuickSlot != null)
                    dbContext.Quickslotkeymappeds.Add(new Quickslotkeymapped(acc.Key, acc.Value.QuickSlot.LongValue));

                dbContext.Storages.Add(new StorageEntity(acc.Key, (int)StorageType.AccountStorage, acc.Value.Storage?.Slots ?? 4, acc.Value.Storage?.Meso ?? 0));

                await InventoryManager.CommitInventoryByTypeAsync(dbContext, acc.Key, acc.Value.Storage?.Items ?? [], ItemFactory.STORAGE);

                await dbContext.Accounts.Where(x => x.Id == acc.Key).ExecuteUpdateAsync(
                    x => x.SetProperty(y => y.MaplePoint, acc.Value.MaplePoint)
                        .SetProperty(y => y.NxPrepaid, acc.Value.NxPrepaid)
                        .SetProperty(y => y.NxCredit, acc.Value.NxCredit)
                    );

                await InventoryManager.CommitInventoryByTypeAsync(dbContext, acc.Key, acc.Value.CashAranItems, ItemFactory.CASH_ARAN);
                await InventoryManager.CommitInventoryByTypeAsync(dbContext, acc.Key, acc.Value.CashCygnusItems, ItemFactory.CASH_CYGNUS);
                await InventoryManager.CommitInventoryByTypeAsync(dbContext, acc.Key, acc.Value.CashExplorerItems, ItemFactory.CASH_EXPLORER);
                await InventoryManager.CommitInventoryByTypeAsync(dbContext, acc.Key, acc.Value.CashOverallItems, ItemFactory.CASH_OVERALL);
            }
            await dbContext.SaveChangesAsync();
        }

        public Task InitializeAsync(DBContext dbContext)
        {
            return Task.CompletedTask;
        }

        public AccountGame? GetAccountGameData(int accountId)
        {
            if (_accGameDataSource.TryGetValue(accountId, out var data) && data != null)
                return data;

            using var dbContext = _dbContextFactory.CreateDbContext();
            var accountData = dbContext.Accounts.FirstOrDefault(x => x.Id == accountId);
            if (accountData == null)
                return null;

            var allAccountItems = _server.InventoryManager.LoadItems(dbContext, true, accountId,
                ItemType.Storage, ItemType.CashAran, ItemType.CashCygnus, ItemType.CashExplorer, ItemType.CashOverall);

            var storage = _mapper.Map<StorageModel>(
                    dbContext.Storages.FirstOrDefault(x => x.OwnerId == accountId && x.Type == (int)StorageType.AccountStorage)
                    ) ?? new StorageModel(accountId, (int)StorageType.AccountStorage);
            storage.Items = allAccountItems.Where(x => x.Type == (int)ItemType.Storage).ToArray();

            data = new AccountGame
            {
                Id = accountData.Id,
                NxCredit = accountData.NxCredit ?? 0,
                MaplePoint = accountData.MaplePoint ?? 0,
                NxPrepaid = accountData.NxPrepaid ?? 0,

                CashOverallItems = allAccountItems.Where(x => x.Type == (int)ItemType.CashOverall).ToArray(),
                CashAranItems = allAccountItems.Where(x => x.Type == (int)ItemType.CashAran).ToArray(),
                CashCygnusItems = allAccountItems.Where(x => x.Type == (int)ItemType.CashCygnus).ToArray(),
                CashExplorerItems = allAccountItems.Where(x => x.Type == (int)ItemType.CashExplorer).ToArray(),
                QuickSlot = _mapper.Map<QuickSlotModel>(dbContext.Quickslotkeymappeds.AsNoTracking().Where(x => x.Accountid == accountId).FirstOrDefault()),
                Storage = storage
            };
            _accGameDataSource[accountId] = data;
            return data;
        }

        public void UpdateAccountGame(AccountGame accountGame)
        {
            _accGameDataSource[accountGame.Id] = accountGame;
            _updated[accountGame.Id] = StoreFlag.AddOrUpdate;
        }
    }
}
