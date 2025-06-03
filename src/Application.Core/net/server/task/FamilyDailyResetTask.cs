using Application.Core.Channel;
using Application.Core.Managers;
using client;

namespace net.server.task;

public class FamilyDailyResetTask : BaseTask
{
    private World world;

    public FamilyDailyResetTask(World world) : base(world)
    {
        this.world = world;
    }

    public override void HandleRun()
    {
        FamilyManager.resetEntitlementUsage(world);
        foreach (Family family in world.getFamilies())
        {
            family.resetDailyReps();
        }
    }
}
