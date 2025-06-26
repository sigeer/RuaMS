using Application.Core.Login.Services;
using Application.Utility.Tasks;

namespace Application.Core.Login.Tasks
{
    public class DueyFredrickTask : AbstractRunnable
    {
        readonly MasterServer _server;
        readonly FredrickService _fredrickService;

        public DueyFredrickTask(FredrickService fredrickProcessor, MasterServer server)
        {
            this._fredrickService = fredrickProcessor;
            _server = server;
        }

        public override void HandleRun()
        {
            _fredrickService.runFredrickSchedule();
        }
    }

}
