using Application.Core.Channel.Commands;
using Application.Shared.Servers;
using Application.Utility.Pipeline;
using Application.Utility.Tickables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel
{
    public interface IChannelServer : ISocketServer, IActorInstance<WorldChannel>, ITickableTree
    {
        int Id { get; }
    }
}
