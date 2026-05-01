using Application.Core.Channel.Commands;
using Application.Core.Game.Invites;

namespace Application.Core.Channel.Tasks
{
    public class InvitationTask : AbstractRunnable
    {
        readonly WorldChannelServer _server;

        public InvitationTask(WorldChannelServer server) : base($"{nameof(InvitationTask)}")
        {
            _server = server;
        }

        public override void HandleRun()
        {
            _server.Send(s =>
            {
                InviteType.TRADE.CheckExpired(s.getCurrentTime());
            });
        }
    }
}
