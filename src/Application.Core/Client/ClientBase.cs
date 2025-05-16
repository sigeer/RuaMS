using Application.Shared.Servers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;

namespace Application.Core.Client
{
    public abstract class ClientBase : SocketClient, IClientBase
    {
        protected ClientBase(long sessionId, IServerBase<IServerTransport> currentServer, IChannel nettyChannel, ILogger<IClientBase> log) : base(sessionId, currentServer, nettyChannel, log)
        {
        }
        public bool IsServerTransition { get; protected set; }

        public abstract int AccountId {  get; }
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
            CurrentServer.UpdateAccountChracterByAdd(AccountId, id);
        }
    }
}
