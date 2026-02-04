using AllianceProto;
using Microsoft.AspNetCore.Hosting.Server;
using net.server.guild;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands.Alliance
{
    internal class UpdateMemberAllianceRankCallbackCommand : IWorldChannelCommand
    {
        ChangePlayerAllianceRankResponse res;

        public UpdateMemberAllianceRankCallbackCommand(ChangePlayerAllianceRankResponse res)
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
                    chr.sendPacket(GuildPackets.GetGuildAlliances(res.AllianceDto));
                    chr.AllianceRank = res.NewRank;
                }
            }
            return;
        }
    }
}
