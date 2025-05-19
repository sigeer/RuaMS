using Application.Core.tools;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Characters;
using Application.Utility.Exceptions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using tools;

namespace Application.Core.Login.Datas
{
    public class AccountManager
    {
        Dictionary<int, AccountDto> _accStorage = new Dictionary<int, AccountDto>();
        Dictionary<int, AccountDto> _needUpdate = new();

        Dictionary<int, HashSet<int>> _accPlayerCache = new();
        readonly ILogger<AccountManager> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _maaper;

        public AccountManager(ILogger<AccountManager> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper maaper)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _maaper = maaper;
        }

        public AccountDto? GetAccountDto(int accId)
        {
            if (_accStorage.TryGetValue(accId, out var account))
            {
                return account;
            }

            var dbModel = GetAccountDtoFromDB(accId);
            if (dbModel != null)
            {
                _accStorage[accId] = dbModel;
                return dbModel;
            }
            return null;
        }


        private AccountDto? GetAccountDtoFromDB(int accId)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbModel = dbContext.Accounts.AsNoTracking().FirstOrDefault(x => x.Id == accId);
            return _maaper.Map<AccountDto>(dbModel);
        }

        public int GetAccountEntityByName(string accName)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return dbContext.Accounts.AsNoTracking().FirstOrDefault(x => x.Name == accName)?.Id ?? -2;
        }

        private readonly object _commitLock = new();
        public async Task CommitAsync()
        {
            AccountEntity[] entities;
            lock (_commitLock)
            {
                if (_needUpdate.Count == 0)
                    return;

                entities = _maaper.Map<AccountEntity[]>(_needUpdate.Values);
                _needUpdate.Clear();
            }

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            dbContext.Accounts.UpdateRange(entities);
            await dbContext.SaveChangesAsync();
        }

        public void UpdateAccountState(int accId, sbyte newState)
        {
            var accountEntity = GetAccountDto(accId);
            if (accountEntity == null)
                throw new BusinessException($"AccountId = {accId} 不存在");

            UpdateAccountState(accountEntity, newState);
        }

        public void UpdateAccountState(AccountDto accountEntity, sbyte newState)
        {
            accountEntity.Loggedin = newState;
            accountEntity.Lastlogin = DateTimeOffset.Now;

            Update(accountEntity);
        }

        public void Update(AccountDto obj)
        {
            _accStorage[obj.Id] = obj;

            _needUpdate[obj.Id] = obj;
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
