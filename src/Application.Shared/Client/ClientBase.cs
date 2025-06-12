using Application.Shared.Models;
using Application.Shared.Servers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;

namespace Application.Shared.Client
{
    public abstract class ClientBase : SocketClient, IClientBase
    {
        public AccountCtrl? AccountEntity { get; set; }
        protected ClientBase(long sessionId, ISocketServer currentServer, IChannel nettyChannel, ILogger<ISocketClient> log) : base(sessionId, nettyChannel, currentServer, log)
        {
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

        public abstract void CommitAccount();
    }
}
