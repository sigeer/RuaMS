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

        public async Task<AccountLoginStatus> UpdateAccountState(int accId, sbyte newState)
        {
            return await AccountManager.UpdateAccountState(accId, newState);
        }

        /// <summary>
        /// 频道服务器发起的账号状态变更。
        /// <para>
        /// 只有这条路径才需要顺带把角色踢下线：玩家在商城/拍卖行里直接断线时，频道侧已经没有
        /// <c>Player</c> 可以同步，中心服缓存的 <c>Channel</c> 会停在非 0 值（只能由这里兜底清理）。
        /// 登录服自己发起的 <see cref="UpdateAccountState"/> 不能这么做 —— 那只是登录连接的状态，
        /// 玩家可能正在游戏里。
        /// </para>
        /// </summary>
        public async Task<AccountLoginStatus> UpdateAccountStateFromChannel(int accId, sbyte newState)
        {
            var data = await UpdateAccountState(accId, newState);

            if (newState == LoginStage.LOGIN_NOTLOGGEDIN)
            {
                await CharacterManager.LogoffAccountCharacters(accId);
            }

            return data;
        }

        public List<CharacterLiveObject> LoadAccountCharactersView(int id)
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
