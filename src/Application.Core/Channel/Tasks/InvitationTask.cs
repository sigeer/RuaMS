using Application.Core.Game.Invites;

namespace Application.Core.Channel.Tasks
{
    public class InvitationTask : ActorTask<WorldChannelServer>
    {
        readonly WorldChannelServer _server;

        public InvitationTask(WorldChannelServer server) : base(server, nameof(InvitationTask), TimeSpan.FromSeconds(30))
        {
            _server = server;
        }

        protected override void HandleRun()
        {
            InviteType.TRADE.CheckExpired(_server.getCurrentTime());
        }
    }
}
