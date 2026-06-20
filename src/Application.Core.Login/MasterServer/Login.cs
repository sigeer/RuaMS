using Application.Core.Login.Client;
using Application.Core.Login.Datas;
using Application.Core.Login.Models;
using Application.Core.Login.Session;
using Application.Shared.Login;
using Application.Utility.Configs;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

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

        public AccountLoginStatus UpdateAccountState(int accId, sbyte newState)
        {
            return AccountManager.UpdateAccountState(accId, newState);
        }

        public List<CharacterViewObject> LoadAccountCharactersView(int id)
        {
            return CharacterManager.GetCharactersView(AccountManager.GetAccountPlayerIds(id).ToArray());
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

        private ConcurrentDictionary<ILoginClient, DateTimeOffset> inLoginState = new();
        public void RegisterLoginState(ILoginClient c)
        {
            inLoginState[c] = DateTimeOffset.UtcNow.AddMinutes(10);
        }

        public void UnregisterLoginState(ILoginClient c)
        {
            inLoginState.TryRemove(c, out _);
        }

        private async Task DisconnectIdlesOnLoginState()
        {
            var timeNow = DateTimeOffset.UtcNow;
            var toDisconnect = inLoginState
                .Where(x => timeNow > x.Value)
                .ToList();

            foreach (var kvp in toDisconnect)
            {
                inLoginState.TryRemove(kvp);
            }

            var sessionCoordinator = ServiceProvider.GetRequiredService<SessionCoordinator>();
            foreach (var c in toDisconnect)
            {
                // thanks Lei for pointing a deadlock issue with srvLock
                if (c.Key.IsOnlined)
                {
                    await c.Key.Disconnect();
                }
                else
                {
                    sessionCoordinator.closeSession(c.Key, true);
                }
            }
        }
    }
}
