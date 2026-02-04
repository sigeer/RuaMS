using GuildProto;
using Microsoft.AspNetCore.Hosting.Server;
using net.server.guild;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class InvokeGuildDisbandCallbackCommand : IWorldChannelCommand
    {
        GuildDisbandResponse res;

        public InvokeGuildDisbandCallbackCommand(GuildDisbandResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            foreach (var memberId in res.AllMembers)
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(memberId);
                if (chr != null)
                {
                    if (chr.GuildId == res.GuildId)
                    {
                        chr.sendPacket(GuildPackets.updateGP(res.GuildId, 0));
                        chr.sendPacket(GuildPackets.ShowGuildInfo(null));

                        chr.getMap().broadcastPacket(chr, GuildPackets.guildNameChanged(chr.Id, ""));
                    }
                    else
                    {
                        if (res.AllianceDto != null)
                        {
                            chr.sendPacket(GuildPackets.UpdateAllianceInfo(res.AllianceDto));
                            chr.sendPacket(GuildPackets.allianceNotice(res.AllianceDto.AllianceId, res.AllianceDto.Notice));
                        }
                    }
                }
            }
        }
    }
}
