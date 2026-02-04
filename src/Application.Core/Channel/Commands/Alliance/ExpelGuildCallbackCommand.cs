using AllianceProto;
using Microsoft.AspNetCore.Hosting.Server;
using net.server.guild;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands.Alliance
{
    internal class ExpelGuildCallbackCommand : IWorldChannelCommand
    {
        AllianceExpelGuildResponse res;

        public ExpelGuildCallbackCommand(AllianceExpelGuildResponse res)
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
                    chr.sendPacket(GuildPackets.RemoveGuildFromAlliance(res.AllianceDto, res.GuildDto));

                    chr.sendPacket(GuildPackets.GetGuildAlliances(res.AllianceDto));
                    chr.sendPacket(GuildPackets.allianceNotice(res.AllianceId, res.AllianceDto.Notice));

                    chr.dropMessage("[" + res.GuildDto.Name + "] guild has been expelled from the union.");
                }
            }

            foreach (var guildMember in res.GuildDto.Members)
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(guildMember.Id);
                if (chr != null)
                {
                    chr.sendPacket(GuildPackets.RemoveGuildFromAlliance(res.AllianceDto, res.GuildDto));

                    chr.sendPacket(GuildPackets.disbandAlliance(res.AllianceId));
                }
            }
            return;
        }
    }
}
