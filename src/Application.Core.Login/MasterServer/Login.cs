using Application.Core.Login.Client;
using Application.Core.Login.Datas;
using Application.Core.Login.Models;
using Application.Core.Login.Session;
using Application.EF.Entities;
using Application.Shared.Login;
using Application.Shared.Message;
using Application.Utility.Configs;
using CreatorProto;
using Google.Protobuf;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using SystemProto;
using XmlWzReader;

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


        private Lock srvLock = new ();

        private Dictionary<ILoginClient, DateTimeOffset> inLoginState = new(100);
        public void RegisterLoginState(ILoginClient c)
        {
            srvLock.Enter();
            try
            {
                inLoginState[c] = DateTimeOffset.UtcNow.AddMinutes(10);
            }
            finally
            {
                srvLock.Exit();
            }
        }

        public void UnregisterLoginState(ILoginClient c)
        {
            srvLock.Enter();
            try
            {
                inLoginState.Remove(c);
            }
            finally
            {
                srvLock.Exit();
            }
        }

        private async Task DisconnectIdlesOnLoginState()
        {
            List<ILoginClient> toDisconnect = new();

            srvLock.Enter();
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
                srvLock.Exit();
            }

            var sessionCoordinator = ServiceProvider.GetRequiredService<SessionCoordinator>();
            foreach (var c in toDisconnect)
            {
                // thanks Lei for pointing a deadlock issue with srvLock
                if (c.IsOnlined)
                {
                    await c.Disconnect();
                }
                else
                {
                    await sessionCoordinator.closeSession(c, true);
                }
            }
        }
    }
}
