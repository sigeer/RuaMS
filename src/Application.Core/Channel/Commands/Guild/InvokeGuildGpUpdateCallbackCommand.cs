using Application.Shared.Team;
using GuildProto;
using Microsoft.AspNetCore.Hosting.Server;
using net.server.guild;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class InvokeGuildGpUpdateCallbackCommand : IWorldChannelCommand
    {
        UpdateGuildCapacityResponse res;

        public InvokeGuildGpUpdateCallbackCommand(UpdateGuildCapacityResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var resCode = (GuildUpdateResult)res.Code;
            if (resCode != GuildUpdateResult.Success)
            {
                var masterChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.Request.MasterId);
                if (masterChr != null)
                {
                    // 失败回滚
                    if (resCode == GuildUpdateResult.GuildFull)
                    {
                        masterChr.Popup("Your guild already reached the maximum capacity of players.");
                    }
                    masterChr.GainMeso(res.Request.Cost);
                }
                return;
            }


            foreach (var memberId in res.GuildMembers)
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(memberId);
                if (chr != null)
                {
                    chr.sendPacket(GuildPackets.guildCapacityChange(res.GuildId, res.GuildCapacity));
                }
            }
            return;
        }
    }
}
