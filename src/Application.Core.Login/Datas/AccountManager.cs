using Application.Core.tools;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Characters;
using Application.Shared.Login;
using Application.Utility.Exceptions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Login.Datas
{
    public class AccountManager
    {
        /// <summary>
        /// 账户登录态记录
        /// </summary>
        Dictionary<int, AccountLoginStatus> _accStageCache = new Dictionary<int, AccountLoginStatus>();

        /// <summary>
        /// 账户及其拥有的角色id缓存
        /// </summary>
        Dictionary<int, HashSet<int>> _accPlayerCache = new();

        readonly ILogger<AccountManager> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _maaper;
        readonly DataStorage _dataStorage;
        public AccountManager(ILogger<AccountManager> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper maaper, DataStorage dataStorage)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _maaper = maaper;
            _dataStorage = dataStorage;
        }

        public AccountDto? GetAccountDto(int accId)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbModel = dbContext.Accounts.AsNoTracking().FirstOrDefault(x => x.Id == accId);
            return _maaper.Map<AccountDto>(dbModel);
        }

        public int GetAccountIdByName(string accName)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return dbContext.Accounts.AsNoTracking().FirstOrDefault(x => x.Name == accName)?.Id ?? -2;
        }

        public AccountLoginStatus GetAccountLoginStatus(int accId)
        {
            if (_accStageCache.TryGetValue(accId, out var d))
                return d;

            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbModel = dbContext.Accounts.AsNoTracking().FirstOrDefault(x => x.Id == accId);
            if (dbModel != null)
            {
                d = new AccountLoginStatus(0, dbModel.Lastlogin ?? DateTimeOffset.MinValue);
                return d;
            }
            else
                throw new BusinessException($"账号不存在，Id = {accId}");
        }

        public void UpdateAccount(AccountDto obj)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbModel = dbContext.Accounts.AsNoTracking().FirstOrDefault(x => x.Id == obj.Id);
            if (dbModel == null)
                return;

            dbModel.Macs = obj.Macs;
            dbModel.Hwid = obj.Hwid;
            dbModel.Pic = obj.Pic;
            dbModel.Pin = obj.Pin;
            dbModel.Ip = obj.Ip;
            dbModel.GMLevel = obj.GMLevel;
            dbModel.Characterslots = obj.Characterslots;
            dbContext.SaveChanges();
        }

        public void UpdateAccountState(int accId, sbyte newState)
        {
            var d = GetAccountLoginStatus(accId);
            d.State = newState;
            d.DateTime = DateTimeOffset.UtcNow;

            _dataStorage.SetAccountLoginRecord(new KeyValuePair<int, AccountLoginStatus>(accId, d));
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
            var e = dbContext.Characters.AsNoTracking().Select(x => x.Id).ToHashSet();
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

    }
}
