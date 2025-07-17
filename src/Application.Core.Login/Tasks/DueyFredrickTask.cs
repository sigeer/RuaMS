using Application.Core.Login.Services;
using Application.Utility.Tasks;

namespace Application.Core.Login.Tasks
{
    public class DueyFredrickTask : AbstractRunnable
    {
        readonly MasterServer _server;
        public DueyFredrickTask(MasterServer server)
        {
            _server = server;
        }

        public override void HandleRun()
        {
            _server.PlayerShopManager.RunFredrickSchedule();
        }
    }

}
