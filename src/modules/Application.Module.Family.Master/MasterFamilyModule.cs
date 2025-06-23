using Application.Core.Login;
using Application.Core.Login.Events;
using Application.EF;
using Application.Module.Family.Master.Tasks;
using Application.Utility;
using Application.Utility.Tasks;
using Microsoft.Extensions.Logging;

namespace Application.Module.Family.Master
{
    public class MasterFamilyModule : MasterModule
    {
        readonly FamilyManager _familyManager;
        readonly DataService _dataService;

        ScheduledFuture? _task;
        public MasterFamilyModule(
            MasterServer server,
            FamilyManager familyManager,
            ILogger<MasterModule> logger,
            DataService dataService) : base(server, logger)
        {
            _familyManager = familyManager;
            _dataService = dataService;
        }

        public override int DeleteCharacterCheck(int id)
        {
            var chr = _server.CharacterManager.FindPlayerById(id);
            if (chr != null)
            {
                var family = _familyManager.GetFamily(chr.Character.FamilyId);
                if (family != null && family.Members.Count > 1)
                    return 0x1D;
            }

            return 0;
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            _familyManager.ResetEntitlementUsage();
            var timeLeft = TimeUtils.GetTimeLeftForNextDay();
            _task = _server.TimerManager.register(new FamilyDailyResetTask(_familyManager), TimeSpan.FromDays(1), timeLeft);
        }

        public override async Task IntializeDatabaseAsync(DBContext dbContext)
        {
            await base.IntializeDatabaseAsync(dbContext);
            await _familyManager.LoadAllFamilyAsync(dbContext);
        }

        public override async Task UninstallAsync()
        {
            await base.UninstallAsync();
            if (_task != null)
                await _task.CancelAsync(false);
        }

        public override async Task SaveChangesAsync(DBContext dbContext)
        {
            await _dataService.CommitAsync(dbContext);
        }
    }
}
