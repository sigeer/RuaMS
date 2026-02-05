using Application.Core.Channel.Commands;

namespace Application.Core.Channel.Tasks
{
    public class InvitationTask : AbstractRunnable
    {
        readonly WorldChannelServer _server;

        public InvitationTask(WorldChannelServer server) : base($"{server.InstanceName}_{nameof(InvitationTask)}")
        {
            _server = server;
        }

        public override void HandleRun()
        {
            _server.Post(new InvitationExpireCheckCommand());
        }
    }
}
