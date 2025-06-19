using Microsoft.Extensions.Logging;

namespace Application.Core.Channel.Invitation
{
    public abstract class InviteChannelHandler
    {
        protected WorldChannelServer _server;
        protected ILogger<InviteChannelHandler> _logger;
        public string Type { get; }
        protected InviteChannelHandler(WorldChannelServer server, string type, ILogger<InviteChannelHandler> logger)
        {
            _server = server;
            Type = type;
            _logger = logger;
        }

        public abstract void OnInvitationCreated(Dto.CreateInviteResponse data);
        public abstract void OnInvitationAnswered(Dto.AnswerInviteResponse data);

    }
}
