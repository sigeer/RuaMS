using Application.Shared.Team;
using GuildProto;
using Microsoft.AspNetCore.Hosting.Server;
using net.server.guild;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class InvokeGuildExpelMemberCallbackCommand : IWorldChannelCommand
    {
        ExpelFromGuildResponse res;

        public InvokeGuildExpelMemberCallbackCommand(ExpelFromGuildResponse res)
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
                    if (resCode == GuildUpdateResult.MasterRankFail)
                    {
                        masterChr.dropMessage(1, "权限不足");
                    }
                }
                return;
            }

            var targetChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.Request.TargetPlayerId);
            if (targetChr != null)
            {
                targetChr.sendPacket(GuildPackets.updateGP(res.GuildId, 0));
                targetChr.sendPacket(GuildPackets.ShowGuildInfo(null));

                targetChr.getMap().broadcastPacket(targetChr, GuildPackets.guildNameChanged(targetChr.Id, ""));
            }

            foreach (var memberId in res.AllLeftMembers)
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(memberId);
                if (chr != null)
                {
                    if (chr.GuildId == res.GuildId)
                    {
                        chr.sendPacket(GuildPackets.memberLeft(res.GuildId, res.Request.TargetPlayerId, res.TargetName, true));
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
            return;
        }
    }
}
