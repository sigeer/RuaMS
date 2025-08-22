using Application.Core.Login.Models;
using Application.Core.Login.Services;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Items;
using Application.Shared.Login;
using Application.Utility;
using Application.Utility.Exceptions;
using Application.Utility.Extensions;
using Dto;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Datas
{
    public class AccountManager
    {
        /// <summary>
        /// 账户登录态记录
        /// </summary>
        Dictionary<int, AccountLoginStatus> _accStageCache = new Dictionary<int, AccountLoginStatus>();

        Dictionary<int, AccountGame> _accGameDataSource = new();
        Dictionary<int, AccountCtrl> _accDataSource = new();

        /// <summary>
        /// 账户及其拥有的角色id缓存
        /// </summary>
        Dictionary<int, HashSet<int>> _accPlayerCache = new();

        readonly ILogger<AccountManager> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _maaper;
        readonly DataStorage _dataStorage;
        readonly MasterServer _server;
        public AccountManager(ILogger<AccountManager> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper maaper, DataStorage dataStorage, MasterServer server)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _maaper = maaper;
            _dataStorage = dataStorage;
            _server = server;
        }

        public AccountCtrl? GetAccountDto(int accId)
        {
            return GetAccount(accId);
        }

        public int GetAccountIdByName(string accName)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return dbContext.Accounts.AsNoTracking().FirstOrDefault(x => x.Name == accName)?.Id ?? -2;
        }

        public AccountLoginStatus GetAccountLoginStatus(int accId)
        {
            return _accStageCache.GetOrAdd(accId, () =>
            {
                using var dbContext = _dbContextFactory.CreateDbContext();
                var dbModel = dbContext.Accounts.AsNoTracking().FirstOrDefault(x => x.Id == accId);
                if (dbModel != null)
                {
                    return new AccountLoginStatus(0, dbModel.Lastlogin ?? DateTimeOffset.MinValue);
                }
                else
                    throw new BusinessException($"账号不存在，Id = {accId}");
            });
        }



        public AccountLoginStatus UpdateAccountState(int accId, sbyte newState)
        {
            var d = GetAccountLoginStatus(accId);
            d.State = newState;
            d.DateTime = _server.GetCurrentTimeDateTimeOffset();

            _dataStorage.SetAccountLoginRecord(new KeyValuePair<int, AccountLoginStatus>(accId, d));
            return d;
        }

        public void CreateAccount(string loginAccount, string pwd)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var password = HashDigest.HashByType("SHA-512", pwd).ToHexString();
            var newAccModel = new AccountEntity(loginAccount, password);
            dbContext.Accounts.Add(newAccModel);
            dbContext.SaveChanges();
        }

        public async Task SetupAccountPlayerCache(DBContext dbContext)
        {
            _accPlayerCache = (await dbContext.Characters.AsNoTracking().Select(x => new { Id = x.Id, AccountId = x.AccountId }).ToListAsync())
                .GroupBy(x => x.AccountId)
                .ToDictionary(x => x.Key, x => x.Select(y => y.Id).ToHashSet());
        }

        public HashSet<int> GetAccountPlayerIds(int accId)
        {
            if (_accPlayerCache.TryGetValue(accId, out var d))
                return d;

            using var dbContext = _dbContextFactory.CreateDbContext();
            var e = dbContext.Characters.Where(x => x.AccountId == accId).AsNoTracking().Select(x => x.Id).ToHashSet();
            _accPlayerCache[accId] = e;
            return e;
        }

        public bool ValidAccountCharacter(int accId, int charId)
        {
            return GetAccountPlayerIds(accId).Contains(charId);
        }

        public void UpdateAccountCharacterCacheByAdd(int accId, int charId)
        {
            if (_accPlayerCache.TryGetValue(accId, out var d))
                d.Add(charId);
            else
            {
                _accPlayerCache[accId] = [charId];
            }
        }

        public void UpdateAccountCharacterCacheByRemove(int accId, int charId)
        {
            if (_accPlayerCache.TryGetValue(accId, out var d))
                d.Remove(charId);
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

            data = new AccountGame
            {
                Id = accountData.Id,
                NxCredit = accountData.NxCredit ?? 0,
                MaplePoint = accountData.MaplePoint ?? 0,
                NxPrepaid = accountData.NxPrepaid ?? 0,

                StorageItems = allAccountItems.Where(x => x.Type == (int)ItemType.Storage).ToArray(),
                CashOverallItems = allAccountItems.Where(x => x.Type == (int)ItemType.CashOverall).ToArray(),
                CashAranItems = allAccountItems.Where(x => x.Type == (int)ItemType.CashAran).ToArray(),
                CashCygnusItems = allAccountItems.Where(x => x.Type == (int)ItemType.CashCygnus).ToArray(),
                CashExplorerItems = allAccountItems.Where(x => x.Type == (int)ItemType.CashExplorer).ToArray(),
                QuickSlot = _maaper.Map<QuickSlotModel>(dbContext.Quickslotkeymappeds.AsNoTracking().Where(x => x.Accountid == accountId).FirstOrDefault()),
                Storage = _maaper.Map<StorageModel>(
                    dbContext.Storages.FirstOrDefault(x => x.Accountid == accountId)
                    ) ?? new StorageModel(accountId)
            };
            _accGameDataSource[accountId] = data;
            return data;
        }
        public void UpdateAccountGame(AccountGame accountGame)
        {
            _accGameDataSource[accountGame.Id] = accountGame;
            _dataStorage.SetAccountGame(accountGame);
        }

        internal AccountCtrl? GetAccount(int accountId)
        {
            if (_accDataSource.TryGetValue(accountId, out var accountCtrl) && accountCtrl != null)
                return accountCtrl;

            using var dbContext = _dbContextFactory.CreateDbContext();
            var accountData = dbContext.Accounts.FirstOrDefault(x => x.Id == accountId);
            if (accountData == null)
                return null;

            accountCtrl = _maaper.Map<AccountCtrl>(accountData);
            _accDataSource[accountId] = accountCtrl;
            return accountCtrl;
        }

        public void UpdateAccount(AccountCtrl obj)
        {
            _accDataSource[obj.Id] = obj;
            _dataStorage.SetAccount(obj);
        }

        public ConfigProto.SetFlyResponse SetFly(ConfigProto.SetFlyRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.CId);
            if (chr != null)
            {
                if (_accDataSource.TryGetValue(chr.Character.AccountId, out var data))
                {
                    data.CanFly = request.SetStatus;

                    return new ConfigProto.SetFlyResponse { Code = 0, Request = request };
                }
            }
            return new ConfigProto.SetFlyResponse() { Code = 1 };
        }

        public int[] GetOnlinedGmAccId()
        {
            return _accDataSource.Values.Where(x => x.GMLevel > 1).Select(x => x.Id).ToArray();
        }

        public SystemProto.SetGmLevelResponse SetGmLevel(SystemProto.SetGmLevelRequest request)
        {
            var targetChr = _server.CharacterManager.FindPlayerByName(request.TargetName);
            if (targetChr == null)
                return new SystemProto.SetGmLevelResponse { Code = 1 };

            var accountDto = GetAccount(targetChr.Character.AccountId)!;
            accountDto.GMLevel = (sbyte)request.Level;

            _server.Transport.BroadcastGmLevelChanged(new SystemProto.SetGmLevelBroadcast
            {
                TargetId = targetChr.Character.Id,
                OperatorName = _server.CharacterManager.GetPlayerName(request.OperatorId),
                TargetName = request.TargetName,
                Level = request.Level
            });
            return new SystemProto.SetGmLevelResponse();
        }

        public GetAllClientInfo GetOnliendClientInfo()
        {
            var onlinedPlayerAccounts = _server.CharacterManager.GetOnlinedPlayerAccountId();
            var accountInfo = _accDataSource.Values.Where(x => onlinedPlayerAccounts.Contains(x.Id));

            var res = new GetAllClientInfo();
            res.List.AddRange(accountInfo.Select(x => new ClientInfo { AccountName = x.Name, CharacterName = "", CurrentHWID = x.CurrentHwid, CurrentIP = x.CurrentIP, CurrentMAC = x.CurrentMac }));
            return res;
        }
    }
}
