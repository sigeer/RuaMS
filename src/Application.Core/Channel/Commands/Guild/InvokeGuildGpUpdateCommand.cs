using GuildProto;
using Microsoft.AspNetCore.Hosting.Server;
using net.server.guild;
using System;
using System.Collections.Generic;
using System.Text;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class InvokeGuildGpUpdateCommand : IWorldChannelCommand
    {
        UpdateGuildGPResponse res;

        public InvokeGuildGpUpdateCommand(UpdateGuildGPResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            foreach (var memberId in res.GuildMembers)
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(memberId);
                if (chr != null)
                {
                    chr.sendPacket(GuildPackets.updateGP(res.GuildId, res.GuildGP));
                    if (res.Request.Gp > 0)
                    {
                        chr.sendPacket(PacketCreator.getGPMessage(res.Request.Gp));
                    }
                }
            }

            return;
        }
    }
}
