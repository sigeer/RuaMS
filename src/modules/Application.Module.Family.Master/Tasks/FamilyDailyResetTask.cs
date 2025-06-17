using Application.Utility.Tasks;

namespace Application.Module.Family.Master.Tasks;

public class FamilyDailyResetTask : AbstractRunnable
{
    readonly FamilyManager _familyManager;

    public FamilyDailyResetTask(FamilyManager familyManager)
    {
        _familyManager = familyManager;
    }

    public override void HandleRun()
    {
        _familyManager.ResetEntitlementUsage();
        _familyManager.ResetDailyReps();
    }
}
