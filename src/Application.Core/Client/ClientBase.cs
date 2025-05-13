using Application.Core.Servers;
using Application.Core.ServerTransports;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Client
{
    public abstract class ClientBase : SocketClient, IClientBase
    {
        protected ClientBase(long sessionId, IServerBase<IServerTransport> currentServer, IChannel nettyChannel, ILogger<IClientBase> log) : base(sessionId, currentServer, nettyChannel, log)
        {
        }

        public AccountEntity? AccountEntity { get; set; }

        public bool CheckBirthday(DateTime date)
        {
            if (AccountEntity == null)
                return false;

            return date.Month == AccountEntity.Birthday.Month && date.Day == AccountEntity.Birthday.Day;
        }

        public bool CheckBirthday(int dateInt)
        {
            if (DateTime.TryParse(dateInt.ToString(), out var d))
                return CheckBirthday(d);
            return false;
        }
    }
}
