using Application.Core.Login.Services;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Constants;
using Application.Shared.Login;
using Application.Shared.Message;
using Application.Utility;
using Application.Utility.Exceptions;
using Application.Utility.Extensions;
using AutoMapper;
using Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Application.Core.Login.Datas
{
    public class AccountManager : IStorage
    {
        /// <summary>
        /// 账户登录态记录
        /// </summary>
        ConcurrentDictionary<int, AccountLoginStatus> _accStageCache = new ();

        ConcurrentDictionary<int, AccountCtrl> _accDataSource = new();
        ConcurrentDictionary<int, StoreFlag> _updated = new();
        /// <summary>
        /// 账户及其拥有的角色id缓存
        /// </summary>
        ConcurrentDictionary<int, HashSet<int>> _accPlayerCache = new();

        readonly ILogger<AccountManager> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _maaper;
        readonly MasterServer _server;
        public AccountManager(ILogger<AccountManager> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper maaper, MasterServer server)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _maaper = maaper;
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
            return _accStageCache.GetOrAdd(accId, (id) =>
            {
                using var dbContext = _dbContextFactory.CreateDbContext();
                var dbModel = dbContext.Accounts.AsNoTracking().FirstOrDefault(x => x.Id == id);
                if (dbModel != null)
                {
                    return new AccountLoginStatus(0, DateTimeOffset.MinValue);
                }
                else
                    throw new BusinessException($"账号不存在，Id = {accId}");
            });
        }



        public AccountLoginStatus UpdateAccountState(int accId, sbyte newState)
        {
            var d = GetAccountLoginStatus(accId);
            d.State = newState;
            d.ProcessTime = _server.GetCurrentTimeDateTimeOffset();
            return d;
        }

        public void SetClientLanguage(int accId, int language)
        {
            var d = GetAccountLoginStatus(accId);
            d.Language = language;
        }

        public void CreateAccount(string loginAccount, string pwd)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var password = HashDigest.HashByType("SHA-512", pwd).ToHexString();
            var newAccModel = new AccountEntity(loginAccount, password);
            dbContext.Accounts.Add(newAccModel);
            dbContext.SaveChanges();
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
            _updated[obj.Id] = StoreFlag.AddOrUpdate;
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
            return _accDataSource.Values.Where(x => x.IsGmAccount()).Select(x => x.Id).ToArray();
        }

        public async Task SetGmLevel(SystemProto.SetGmLevelRequest request)
        {
            var res = new SystemProto.SetGmLevelResponse { Request = request };
            var targetChr = _server.CharacterManager.FindPlayerByName(request.TargetName);
            if (targetChr == null)
            {
                res.Code = 1;
                await _server.Transport.SendMessageN(ChannelRecvCode.InvokeSetGmLevel, res, [request.OperatorId]);
                return;
            }

            var accountDto = GetAccount(targetChr.Character.AccountId)!;
            accountDto.GMLevel = (sbyte)request.Level;
            UpdateAccount(accountDto);

            res.TargetId = targetChr.Character.Id;
            await _server.Transport.SendMessageN(ChannelRecvCode.InvokeSetGmLevel, res, [request.OperatorId, res.TargetId]);
        }

        public bool GainCharacterSlot(int accId)
        {
            var acc = GetAccount(accId)!;
            if (acc.Characterslots < Limits.MaxCharacterSlots)
            {
                acc.Characterslots += 1;
                UpdateAccount(acc);

                return true;
            }
            return false;
        }

        public GetAllClientInfo GetOnliendClientInfo()
        {
            var onlinedPlayerAccounts = _server.CharacterManager.GetOnlinedPlayerAccountId();
            var accountInfo = _accDataSource.Values.Where(x => onlinedPlayerAccounts.Contains(x.Id));

            var res = new GetAllClientInfo();
            res.List.AddRange(accountInfo.Select(x => new ClientInfo { AccountName = x.Name, CharacterName = "", CurrentHWID = x.CurrentHwid, CurrentIP = x.CurrentIP, CurrentMAC = x.CurrentMac }));
            return res;
        }

        public bool TryGetGMInfo(int accId, out int gmLevel)
        {
            gmLevel = 0;
            var acc = GetAccountDto(accId);
            if (acc == null)
                return false;

            gmLevel = acc.GMLevel;

            return acc.IsGmAccount();
        }

        public async Task InitializeAsync(DBContext dbContext)
        {
            _accPlayerCache = new ((await dbContext.Characters.AsNoTracking().Select(x => new { Id = x.Id, AccountId = x.AccountId }).ToListAsync())
                .GroupBy(x => x.AccountId)
                .ToDictionary(x => x.Key, x => x.Select(y => y.Id).ToHashSet()));
        }

        public async Task Commit(DBContext dbContext)
        {
            var updatedItems = _updated.Keys.ToList();
            _updated.Clear();
            if (updatedItems.Count == 0)
                return;

            var trackingItems = _accDataSource.Where(x => updatedItems.Contains(x.Key)).Select(x => x.Value).ToList();
            var allAccounts = await dbContext.Accounts.Where(x => updatedItems.Contains(x.Id)).ToListAsync();
            foreach (var obj in trackingItems)
            {
                var dbModel = allAccounts.FirstOrDefault(x => x.Id == obj.Id);
                if (dbModel != null)
                {
                    dbModel.Pic = obj.Pic;
                    dbModel.Pin = obj.Pin;
                    dbModel.Gender = obj.Gender;
                    dbModel.Tos = obj.Tos;
                    dbModel.GMLevel = obj.GMLevel;
                    dbModel.Characterslots = obj.Characterslots;
                }
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
