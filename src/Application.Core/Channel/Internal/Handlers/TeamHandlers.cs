using Application.Core.Channel.Commands;
using Application.Shared.Message;
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

            protected override void HandleMessage(UpdateTeamResponse res)
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

                _server.PushChannelCommand(new InvokeTeamUpdateCommand(res));
            }

            protected override UpdateTeamResponse Parse(ByteString data) => UpdateTeamResponse.Parser.ParseFrom(data);
        }

        public class Create : InternalSessionChannelHandler<CreateTeamResponse>
        {
            public Create(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnTeamCreated;

            protected override void HandleMessage(CreateTeamResponse res)
            {
                if (res.Code != 0)
                {
                    return;
                }

                _server.TeamManager.SetTeam(res.TeamDto);

                _server.PushChannelCommand(new InvokeCreateTeamCommand(res));
            }

            protected override CreateTeamResponse Parse(ByteString data) => CreateTeamResponse.Parser.ParseFrom(data);
        }
    }
}
