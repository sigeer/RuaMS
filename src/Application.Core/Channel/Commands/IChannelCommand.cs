using Application.Utility.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    public interface IChannelCommand: ICommand<ChannelNodeCommandContext>
    {
    }

    public interface IWorldChannelCommand : ICommand<ChannelCommandContext>
    {
    }
}
