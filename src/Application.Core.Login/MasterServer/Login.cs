using Application.Core.Client;
using Application.EF;
using Application.EF.Entities;
using Application.Utility.Configs;
using Microsoft.EntityFrameworkCore;

namespace Application.Core.Login
{
    public partial class MasterServer
    {
        Dictionary<int, AccountEntity> _accStorage = new Dictionary<int, AccountEntity>();
        public AccountEntity? GetAccountEntity(int accId)
        {
            if (_accStorage.TryGetValue(accId, out var account)) 
            {
                return account;
            }

            using var dbContext = new DBContext();
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
                // rules out possibility of multiple account entries

                accountEntity.Loggedin = newState;
                accountEntity.Lastlogin = DateTimeOffset.Now;
            }
        }


        ReaderWriterLockSlim lgnLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private Dictionary<string, int> transitioningChars = new();
        public void SetCharacteridInTransition(string clientSession, int charId)
        {
            lgnLock.EnterWriteLock();
            try
            {
                transitioningChars[clientSession] = charId;
            }
            finally
            {
                lgnLock.ExitWriteLock();
            }
        }

        public bool ValidateCharacteridInTransition(string clientSession, int charId)
        {
            if (!YamlConfig.config.server.USE_IP_VALIDATION)
            {
                return true;
            }

            lgnLock.EnterWriteLock();
            try
            {
                return transitioningChars.Remove(clientSession, out var cid) && cid == charId;
            }
            finally
            {
                lgnLock.ExitWriteLock();
            }
        }

        public bool HasCharacteridInTransition(string clientSession)
        {
            if (!YamlConfig.config.server.USE_IP_VALIDATION)
            {
                return true;
            }

            lgnLock.EnterReadLock();
            try
            {
                return transitioningChars.ContainsKey(clientSession);
            }
            finally
            {
                lgnLock.ExitReadLock();
            }
        }


        private object srvLock = new object();

        private Dictionary<ILoginClient, DateTimeOffset> inLoginState = new(100);
        public void RegisterLoginState(ILoginClient c)
        {
            Monitor.Enter(srvLock);
            try
            {
                inLoginState[c] = DateTimeOffset.Now.AddMinutes(10);
            }
            finally
            {
                Monitor.Exit(srvLock);
            }
        }

        public void UnregisterLoginState(ILoginClient c)
        {
            Monitor.Enter(srvLock);
            try
            {
                inLoginState.Remove(c);
            }
            finally
            {
                Monitor.Exit(srvLock);
            }
        }

    }
}
