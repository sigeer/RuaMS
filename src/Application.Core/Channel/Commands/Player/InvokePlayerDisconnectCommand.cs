using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class InvokePlayerDisconnectCommand : IWorldChannelCommand
    {
        int _chrId;

        public InvokePlayerDisconnectCommand(int chrId)
        {
            _chrId = chrId;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(_chrId);
            if (chr != null)
            {
                chr.Client.Disconnect(false, false);
            }
            
        }
    }
}
