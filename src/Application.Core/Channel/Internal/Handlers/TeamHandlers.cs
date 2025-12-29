using Application.Shared.Internal;
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

            public override int MessageId => ChannelRecvCode.OnTeamUpdate;

            protected override Task HandleAsync(UpdateTeamResponse res, CancellationToken cancellationToken = default)
            {
                _server.TeamManager.ProcessUpdateResponse(res);
                return Task.CompletedTask;
            }

            protected override UpdateTeamResponse Parse(ByteString data) => UpdateTeamResponse.Parser.ParseFrom(data);
        }
    }
}
