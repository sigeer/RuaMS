using Application.Core.Game.TheWorld;
using Application.Core.Managers;

namespace net.server.task;

public class FamilyDailyResetTask : TimelyControllerBase
{
    readonly IWorldChannel worldChannel;
    public FamilyDailyResetTask(IWorldChannel world) : base("FamilyDailyResetTask", TimeSpan.FromDays(1), TimeSpan.FromDays(1))
    {
        worldChannel = world;
    }

    protected override void HandleRun()
    {
        FamilyManager.resetEntitlementUsage();
        worldChannel.Transport.ResetFamilyDailyReps();
    }
}
