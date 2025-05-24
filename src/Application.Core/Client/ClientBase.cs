using Application.Shared.Servers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;

namespace Application.Core.Client
{
    public abstract class ClientBase : SocketClient, IClientBase
    {
        public IServerBase<IServerTransport> CurrentServerBase { get; protected set; }
        protected ClientBase(long sessionId, IServerBase<IServerTransport> currentServer, IChannel nettyChannel, ILogger<IClientBase> log) : base(sessionId, nettyChannel, log)
        {
            CurrentServerBase = currentServer;
        }
        public bool IsServerTransition { get; protected set; }

        public abstract int AccountId { get; }
        public abstract int AccountGMLevel { get; }

        public abstract string AccountName { get; }

        /// <summary>
        /// q
        /// </summary>
        /// <param name="cid"></param>
        public abstract void SetCharacterOnSessionTransitionState(int cid);
        protected abstract HashSet<string> GetMac();


        public abstract int GetAvailableCharacterSlots();
        public void UpdateAccountChracterByAdd(int id)
        {
            CurrentServerBase.UpdateAccountChracterByAdd(AccountId, id);
        }
    }
}
