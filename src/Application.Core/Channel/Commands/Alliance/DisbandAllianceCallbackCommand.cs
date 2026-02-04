using AllianceProto;
using Microsoft.AspNetCore.Hosting.Server;
using net.server.guild;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class DisbandAllianceCallbackCommand : IWorldChannelCommand
    {
        DisbandAllianceResponse res;

        public DisbandAllianceCallbackCommand(DisbandAllianceResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (res.Code != 0)
            {
                return;
            }

            foreach (var memberId in res.AllMembers)
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(memberId);
                if (chr != null)
                {
                    chr.sendPacket(GuildPackets.disbandAlliance(res.AllianceId));
                    chr.AllianceRank = 5;
                }
            }
            return;

        }
    }
}
