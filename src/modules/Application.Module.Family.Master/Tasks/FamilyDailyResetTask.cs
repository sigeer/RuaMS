using Application.Core.Login;
using Application.Utility.Tasks;

namespace Application.Module.Family.Master.Tasks;

public class FamilyDailyResetTask : ActorTask<MasterServer>
{
    readonly FamilyManager _familyManager;

    public FamilyDailyResetTask(MasterServer server, FamilyManager familyManager) : base(server, nameof(FamilyDailyResetTask), TimeSpan.FromDays(1))
    {
        _familyManager = familyManager;
    }

    protected override void HandleRun()
    {
        _familyManager.ResetEntitlementUsage();
        _familyManager.ResetDailyReps();
    }
}
