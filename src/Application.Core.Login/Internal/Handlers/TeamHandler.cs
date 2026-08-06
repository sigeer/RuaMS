using Application.Shared.Message;
using Application.Shared.Team;
using Google.Protobuf;

namespace Application.Core.Login.Internal.Handlers
{
    internal class TeamHandler
    {
        internal class UpdateHandler : InternalSessionMasterHandler<ProtoService.UpdateTeamRequest>
        {
            public UpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateTeam;

            protected override Task HandleMessage(ProtoService.UpdateTeamRequest message)
            {
                return _server.TeamManager.UpdateParty(message.TeamId, (PartyOperation)message.Operation, message.FromId, message.TargetId);
            }

            protected override ProtoService.UpdateTeamRequest Parse(ByteString content) => ProtoService.UpdateTeamRequest.Parser.ParseFrom(content);
        }
    }
}
