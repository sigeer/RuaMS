using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Players;
using Application.Shared.Internal;
using Application.Shared.Message;
using Application.Shared.Team;
using Google.Protobuf;
using Microsoft.AspNetCore.Hosting.Server;
using System.IO;
using TeamProto;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class TeamHandlers
    {
        public class Update : InternalSessionChannelHandler<UpdateTeamResponse>
        {
            public Update(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnTeamUpdate;

            protected override async Task HandleAsync(UpdateTeamResponse res, CancellationToken cancellationToken = default)
            {
                await _server.TeamManager.ProcessUpdateResponse(res);
            }

            protected override UpdateTeamResponse Parse(ByteString data) => UpdateTeamResponse.Parser.ParseFrom(data);
        }

        public class Create : InternalSessionChannelHandler<CreateTeamResponse>
        {
            public Create(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnTeamCreated;

            protected override Task HandleAsync(CreateTeamResponse res, CancellationToken cancellationToken = default)
            {
                if (res.Code != 0)
                {
                    return Task.CompletedTask;
                }

                _server.TeamManager.SetTeam(res.TeamDto);

                var player = _server.FindPlayerById(res.Request.LeaderId);
                if (player != null)
                {
                    player.sendPacket(TeamPacketCreator.UpdateParty(player.getChannelServer(), res.TeamDto, PartyOperation.SILENT_UPDATE, player.Id, player.Name));

                    player.HandleTeamMemberCountChanged(null);

                    player.sendPacket(TeamPacketCreator.PartyCreated(res.TeamDto.Id, player));
                }

                return Task.CompletedTask;
            }

            protected override CreateTeamResponse Parse(ByteString data) => CreateTeamResponse.Parser.ParseFrom(data);
        }
    }
}
