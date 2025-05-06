using Application.Core.Game.TheWorld;
using Application.Core.Gameplay;

namespace net.server.task;

public class FishingController : TimelyControllerBase
{

    protected override void HandleRun()
    {
        // wserv.FishingInstance.RunCheckFishingSchedule();
    }

    public FishingController(IWorldChannel world) : base("FishingController", TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10))
    {
    }
}
