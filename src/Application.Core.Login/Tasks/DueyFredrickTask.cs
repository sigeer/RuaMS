using Application.Core.Login.Services;
using Application.Utility.Tasks;

namespace Application.Core.Login.Tasks
{
    public class DueyFredrickTask : ActorTask<MasterServer>
    {
        readonly MasterServer _server;
        public DueyFredrickTask(MasterServer server) : base(server, nameof(DueyFredrickTask), TimeSpan.FromHours(1))
        {
            _server = server;
        }

        protected override void HandleRun()
        {
            _server.PlayerShopManager.RunFredrickSchedule();
            _server.DueyManager.RunDueyExpireSchedule();
        }
    }

}
