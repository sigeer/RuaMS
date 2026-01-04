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

            public override int MessageId => ChannelRecvCode.OnTeamUpdate;

            protected override Task HandleAsync(UpdateTeamResponse res, CancellationToken cancellationToken = default)
            {
                _server.TeamManager.ProcessUpdateResponse(res);
                return Task.CompletedTask;
            }

            protected override UpdateTeamResponse Parse(ByteString data) => UpdateTeamResponse.Parser.ParseFrom(data);
        }

        public class Create : InternalSessionChannelHandler<CreateTeamResponse>
        {
            public Create(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.OnTeamCreated;

            protected override async Task HandleAsync(CreateTeamResponse res, CancellationToken cancellationToken = default)
            {
                var player = _server.FindPlayerById(res.Request.LeaderId);
                if (player != null)
                {
                    await _server.TeamManager.UpdateTeam(res.TeamId, PartyOperation.SILENT_UPDATE, player, player.Id);
                    
                    player.HandleTeamMemberCountChanged(null);

                    player.sendPacket(TeamPacketCreator.PartyCreated(res.TeamId, player));
                }
            }

            protected override CreateTeamResponse Parse(ByteString data) => CreateTeamResponse.Parser.ParseFrom(data);
        }
    }
}
