using Application.Core.Channel.Commands;
using Application.Shared.Servers;
using Application.Utility.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel
{
    public interface IChannelServer : ISocketServer, IActor<ChannelCommandContext>
    {
        int Id { get; }
    }
}
