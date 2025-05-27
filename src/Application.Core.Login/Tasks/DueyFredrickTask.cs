using Application.Core.Login.Services;
using Application.Utility.Tasks;

namespace Application.Core.Login.Tasks
{
    public class DueyFredrickTask : AbstractRunnable
    {
        readonly DueyService _dueyService;
        readonly FredrickService _fredrickService;

        public DueyFredrickTask(FredrickService fredrickProcessor, DueyService dueyService)
        {
            this._fredrickService = fredrickProcessor;
            _dueyService = dueyService;
        }

        public override void HandleRun()
        {
            _fredrickService.runFredrickSchedule();
            _dueyService.runDueyExpireSchedule();
        }
    }

}
