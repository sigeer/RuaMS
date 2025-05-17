using Application.Core.tools;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Characters;
using AutoMapper;
using client.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Policy;
using tools;

namespace Application.Core.Login.Datas
{
    public class AccountManager
    {
        Dictionary<int, AccountEntity> _accStorage = new Dictionary<int, AccountEntity>();

        Dictionary<int, HashSet<int>> _accPlayerCache = new ();
        readonly ILogger<AccountManager> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _maaper;

        public AccountManager(ILogger<AccountManager> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper maaper)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _maaper = maaper;
        }

        public AccountEntity? GetAccountEntity(int accId)
        {
            if (_accStorage.TryGetValue(accId, out var account))
            {
                return account;
            }

            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbModel = dbContext.Accounts.AsNoTracking().FirstOrDefault(x => x.Id == accId);
            if (dbModel != null)
            {
                _accStorage[accId] = dbModel;
            }
            return dbModel;
        }

        public int GetAccountEntityByName(string accName)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return dbContext.Accounts.AsNoTracking().FirstOrDefault(x => x.Name == accName)?.Id ?? -2;
        }

        public AccountDto? GetAccountDto(int accId)
        {
            return _maaper.Map<AccountDto>(GetAccountEntity(accId));
        }

        public void UpdateAccountState(int accId, sbyte newState)
        {
            if (_accStorage.TryGetValue(accId, out var accountEntity))
            {
                UpdateAccountState(accountEntity, newState);
            }
        }

        /// <summary>
        /// accountEntity必须是GetAccountEntity返回的
        /// </summary>
        /// <param name="accountEntity"></param>
        /// <param name="newState"></param>
        public void UpdateAccountState(AccountEntity accountEntity, sbyte newState)
        {
            accountEntity.Loggedin = newState;
            accountEntity.Lastlogin = DateTimeOffset.Now;
        }
        public void CreateAccount(string loginAccount, string pwd)
        {
            using var dbContext = new DBContext();
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
            var e  = dbContext.Characters.AsNoTracking().Select(x => x.Id).ToHashSet();
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
        }

        public void UpdateAccountCharacterCacheByRemove(int accId, int charId)
        {
            if (_accPlayerCache.TryGetValue(accId, out var d))
                d.Remove(charId);
        }

    }
}
