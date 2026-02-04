using Application.Core.Channel.Net.Packets;
using Application.Shared.Team;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;
using TeamProto;

namespace Application.Core.Channel.Commands
{
    internal class InvokeCreateTeamCommand : IWorldChannelCommand
    {
        CreateTeamResponse res;

        public InvokeCreateTeamCommand(CreateTeamResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var player = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.Request.LeaderId);
            if (player != null)
            {
                player.Party = res.TeamDto.Id;

                player.sendPacket(TeamPacketCreator.UpdateParty(player.getChannelServer(), res.TeamDto, PartyOperation.SILENT_UPDATE, player.Id, player.Name));

                player.HandleTeamMemberCountChanged(null);

                player.sendPacket(TeamPacketCreator.PartyCreated(res.TeamDto.Id, player));
            }
        }
    }
}
