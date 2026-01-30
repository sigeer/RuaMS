using Microsoft.Extensions.Logging;

namespace Application.Core.Channel.Invitation
{
    public abstract class InviteChannelHandler
    {
        protected WorldChannel _server;
        protected ILogger<InviteChannelHandler> _logger;
        public string Type { get; }
        protected InviteChannelHandler(WorldChannel server, string type, ILogger<InviteChannelHandler> logger)
        {
            _server = server;
            Type = type;
            _logger = logger;
        }

        public abstract void OnInvitationCreated(InvitationProto.CreateInviteResponse data);
        public abstract void OnInvitationAnswered(InvitationProto.AnswerInviteResponse data);

    }
}
