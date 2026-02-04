using Application.Shared.Team;
using GuildProto;
using Microsoft.AspNetCore.Hosting.Server;
using net.server.guild;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class InvokeGuildMemberJoinCallbackCommand : IWorldChannelCommand
    {
        JoinGuildResponse res;

        public InvokeGuildMemberJoinCallbackCommand(JoinGuildResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var resCode = (GuildUpdateResult)res.Code;
            if (resCode != GuildUpdateResult.Success)
            {
                var masterChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.Request.PlayerId);
                if (masterChr != null)
                {
                    if (resCode == GuildUpdateResult.GuildFull)
                    {
                        masterChr.dropMessage(1, "The guild you are trying to join is already full.");
                    }
                }
                return;
            }

            var newMember = res.GuildDto.Members.FirstOrDefault(x => x.Id == res.Request.PlayerId)!;
            foreach (var memberId in res.AllMembers)
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(memberId);
                if (chr != null)
                {
                    if (chr.Id == newMember.Id)
                    {
                        chr.GuildRank = newMember.GuildRank;

                        chr.sendPacket(GuildPackets.ShowGuildInfo(res.GuildDto));

                        chr.getMap().broadcastPacket(chr, GuildPackets.guildNameChanged(chr.Id, res.GuildDto.Name));
                        chr.getMap().broadcastPacket(chr, GuildPackets.guildMarkChanged(chr.Id, res.GuildDto.LogoBg, res.GuildDto.LogoBgColor, res.GuildDto.Logo, res.GuildDto.LogoColor));
                    }
                    else if (chr.GuildId == res.Request.GuildId)
                    {
                        chr.sendPacket(GuildPackets.newGuildMember(res.Request.GuildId,
                            newMember.Id,
                            newMember.Name,
                            newMember.Job,
                            newMember.Level,
                            newMember.GuildRank,
                            newMember.Channel));
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
