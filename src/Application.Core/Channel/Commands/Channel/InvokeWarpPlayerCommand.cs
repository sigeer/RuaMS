using Application.Resources.Messages;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;
using SystemProto;

namespace Application.Core.Channel.Commands
{
    internal class InvokeWarpPlayerCommand : IWorldChannelCommand
    {
        WrapPlayerByNameResponse res;

        public InvokeWarpPlayerCommand(WrapPlayerByNameResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.Request.MasterId);
            if (chr != null)
            {
                if (res.Code != 0)
                {
                    chr.Yellow(nameof(ClientMessage.PlayerNotOnlined), res.Request.Victim);
                }
                else
                {
                    chr.Client.ChangeChannel(res.TargetChannel);
                }
            }
            return;
        }
    }
}
