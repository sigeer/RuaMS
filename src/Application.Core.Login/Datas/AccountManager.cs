using Application.Core.tools;
using Application.EF;
using Application.EF.Entities;
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

        public AccountManager(ILogger<AccountManager> logger, IDbContextFactory<DBContext> dbContextFactory)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
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
