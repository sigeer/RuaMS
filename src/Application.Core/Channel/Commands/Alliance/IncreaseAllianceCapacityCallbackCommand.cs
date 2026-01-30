using AllianceProto;
using Microsoft.AspNetCore.Hosting.Server;
using net.server.guild;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands.Alliance
{
    internal class IncreaseAllianceCapacityCallbackCommand : IWorldChannelCommand
    {
        IncreaseAllianceCapacityResponse res;

        public IncreaseAllianceCapacityCallbackCommand(IncreaseAllianceCapacityResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (res.Code != 0)
            {
                return;
            }

            foreach (var member in res.AllianceDto.Guilds.SelectMany(x => x.Members))
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(member.Id);
                if (chr != null)
                {
                    // 提升了容量，但是这两个数据包都与容量无关
                    //chr.sendPacket(GuildPackets.getGuildAlliances(alliance));
                    //chr.sendPacket(GuildPackets.allianceNotice(alliance.AllianceId, alliance.Notice));

                    if (chr.Id == res.Request.MasterId)
                    {
                        chr.sendPacket(GuildPackets.UpdateAllianceInfo(res.AllianceDto));
                    }
                }
            }

            return;
        }
    }
}
