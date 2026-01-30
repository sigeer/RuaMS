using Application.Shared.Message;
using Application.Shared.Team;
using Google.Protobuf;
using TeamProto;

namespace Application.Core.Login.Internal.Handlers
{
    internal class TeamHandler
    {
        internal class UpdateHandler : InternalSessionMasterHandler<UpdateTeamRequest>
        {
            public UpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateTeam;

            protected override void HandleMessage(UpdateTeamRequest message)
            {
                _ = _server.TeamManager.UpdateParty(message.TeamId, (PartyOperation)message.Operation, message.FromId, message.TargetId);
            }

            protected override UpdateTeamRequest Parse(ByteString content) => UpdateTeamRequest.Parser.ParseFrom(content);
        }
    }
}
