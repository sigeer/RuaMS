using Application.Core.Channel.Commands;
using Application.Core.Channel.Net.Packets;
using Application.Shared.Message;
using Application.Shared.Team;
using Google.Protobuf;
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

            protected override Task HandleMessage(UpdateTeamResponse res)
            {
                if (res.Code == 0)
                {
                    if (res.Request.Operation == 3)
                    {
                        _server.TeamManager.ClearTeamCache(res.Request.TeamId);
                    }
                    else
                    {
                        _server.TeamManager.SetTeam(res.Team);
                    }
                }

                return _server.PushChannelCommandAsync(new InvokeTeamUpdateCommand(res));
            }

            protected override UpdateTeamResponse Parse(ByteString data) => UpdateTeamResponse.Parser.ParseFrom(data);
        }

        public class Create : InternalSessionChannelHandler<CreateTeamResponse>
        {
            public Create(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnTeamCreated;

            protected override Task HandleMessage(CreateTeamResponse res)
            {
                if (res.Code != 0)
                {
                    return Task.CompletedTask;
                }

                _server.TeamManager.SetTeam(res.TeamDto);

                return _server.SendToPlayerAsync(res.Request.LeaderId, async chr =>
                {
                    chr.Party = res.TeamDto.Id;

                    await chr.SendPacket(TeamPacketCreator.UpdateParty(chr.getChannelServer(), res.TeamDto, PartyOperation.SILENT_UPDATE, chr.Id, chr.Name));

                    await chr.HandleTeamMemberCountChanged(null);

                    await chr.SendPacket(TeamPacketCreator.PartyCreated(res.TeamDto.Id, chr));
                });
            }

            protected override CreateTeamResponse Parse(ByteString data) => CreateTeamResponse.Parser.ParseFrom(data);
        }
    }
}
