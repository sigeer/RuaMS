using Application.Core.Login.Client;
using Application.Core.Login.Datas;
using Application.Core.Login.Models;
using Application.Core.Login.Session;
using Application.Shared.Login;
using Application.Shared.Models;
using Application.Utility.Configs;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Login
{
    public partial class MasterServer
    {
        public int GetAccountCharacterCount(int accId)
        {
            return AccountManager.GetAccountPlayerIds(accId).Count;
        }
        public AccountCtrl? GetAccountDto(int accId)
        {
            return AccountManager.GetAccountDto(accId);
        }

        public int GetAccountIdByAccountName(string name)
        {
            return AccountManager.GetAccountIdByName(name);
        }

        public AccountLoginStatus UpdateAccountState(int accId, sbyte newState)
        {
            return AccountManager.UpdateAccountState(accId, newState);
        }

        public List<CharacterViewObject> LoadAccountCharactersView(int id)
        {
            return CharacterManager.GetCharactersView(AccountManager.GetAccountPlayerIds(id).ToArray());
        }

        public void UpdateAccountChracterByAdd(int accountId, int id)
        {
            AccountManager.UpdateAccountCharacterCacheByAdd(accountId, id);
        }

        public AccountLoginStatus GetAccountLoginStatus(int accId)
        {
            return AccountManager.GetAccountLoginStatus(accId);
        }

        public void CommitAccountEntity(AccountCtrl accountEntity)
        {
            AccountManager.UpdateAccount(accountEntity);
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
                inLoginState[c] = DateTimeOffset.UtcNow.AddMinutes(10);
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

        private void DisconnectIdlesOnLoginState()
        {
            List<ILoginClient> toDisconnect = new();

            Monitor.Enter(srvLock);
            try
            {
                var timeNow = DateTimeOffset.UtcNow;

                foreach (var mc in inLoginState)
                {
                    if (timeNow > mc.Value)
                    {
                        toDisconnect.Add(mc.Key);
                    }
                }

                foreach (var c in toDisconnect)
                {
                    inLoginState.Remove(c);
                }
            }
            finally
            {
                Monitor.Exit(srvLock);
            }

            var sessionCoordinator = ServiceProvider.GetRequiredService<SessionCoordinator>();
            foreach (var c in toDisconnect)
            {
                // thanks Lei for pointing a deadlock issue with srvLock
                if (c.IsOnlined)
                {
                    c.Disconnect();
                }
                else
                {
                    sessionCoordinator.closeSession(c, true);
                }
            }
        }

    }
}
