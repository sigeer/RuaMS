using Application.Core.Login;
using Application.Core.Login.Modules;
using Application.Module.Family.Master.Tasks;
using Application.Utility;
using Application.Utility.Tasks;
using Microsoft.Extensions.Logging;

namespace Application.Module.Family.Master
{
    public class MasterFamilyModule : AbstractMasterModule
    {
        readonly FamilyManager _familyManager;

        ScheduledFuture? _task;
        public MasterFamilyModule(
            MasterServer server,
            FamilyManager familyManager,
            ILogger<MasterModule> logger) : base(server, logger)
        {
            _familyManager = familyManager;
        }

        public override int DeleteCharacterCheck(int id)
        {
            var family = _familyManager.GetFamilyByCharacterId(id);
            if (family != null && family.Members.Count > 1)
                return 0x1D;

            return 0;
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            _familyManager.ResetEntitlementUsage();

        }

        public override void RegisterTask(ITimerManager timerManager)
        {
            base.RegisterTask(timerManager);
            var timeLeft = TimeUtils.GetTimeLeftForNextDay();
            _task = timerManager.register(new FamilyDailyResetTask(_familyManager), TimeSpan.FromDays(1), timeLeft);
        }


        public override async Task UninstallAsync()
        {
            await base.UninstallAsync();
            if (_task != null)
                await _task.CancelAsync(false);
        }
    }
}
