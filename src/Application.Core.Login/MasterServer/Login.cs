using Application.Core.Client;
using Application.Core.Game.Players;
using Application.Core.Login.Datas;
using Application.Core.Login.Session;
using Application.EF.Entities;
using Application.Utility.Configs;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Login
{
    public partial class MasterServer
    {
        AccountManager accountManager;
        public int GetAccountCharacterCount(int accId)
        {
            return accountManager.GetAccountPlayerIds(accId).Count;
        }
        public AccountEntity? GetAccountEntity(int accId)
        {
            return accountManager.GetAccountEntity(accId);
        }

        public int GetAccountIdByAccountName(string name)
        {
            return accountManager.GetAccountEntityByName(name);
        }

        public void UpdateAccountState(int accId, sbyte newState)
        {
            accountManager.UpdateAccountState(accId, newState);
        }

        public List<IPlayer> LoadAccountCharactersView(int id)
        {
            return _characterSevice.GetCharactersView(accountManager.GetAccountPlayerIds(id).ToArray());
        }

        public void UpdateAccountChracterByAdd(int accountId, int id)
        {
            accountManager.UpdateAccountCharacterCacheByAdd(accountId, id);
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

        private void DisconnectIdlesOnLoginState()
        {
            List<ILoginClient> toDisconnect = new();

            Monitor.Enter(srvLock);
            try
            {
                var timeNow = DateTimeOffset.Now;

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
