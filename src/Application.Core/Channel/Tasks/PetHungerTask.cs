using Application.Core.Channel.Commands;

namespace Application.Core.Channel.ServerData
{
    public class PetHungerTask : TaskBase
    {

        readonly WorldChannelServer _server;

        public PetHungerTask(WorldChannelServer server) : base($"{server.ServerName}_{nameof(PetHungerManager)}", TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1))
        {
            this._server = server;
        }

        protected override void HandleRun()
        {
            _server.PushChannelCommand(new InvokePetHungerCommand());
        }
    }
}
