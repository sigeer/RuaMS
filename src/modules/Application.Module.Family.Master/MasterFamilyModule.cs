using Application.Core.Login;
using Application.Core.Login.Events;
using Application.Core.Login.Models.Invitations;
using Application.EF;
using Application.Module.Family.Master.Tasks;
using Application.Utility;
using Microsoft.Extensions.Logging;
using server;

namespace Application.Module.Family.Master
{
    public class MasterFamilyModule : MasterModule
    {
        readonly FamilyManager _familyManager;
        readonly DataService _dataService;

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

        public override void Initialize()
        {
            _familyManager.LoadAllFamily();

            var timeLeft = TimeUtils.GetTimeLeftForNextDay();
            _familyManager.ResetEntitlementUsage();
            TimerManager.getInstance().register(new FamilyDailyResetTask(_familyManager), TimeSpan.FromDays(1), timeLeft);
        }

        public override async Task SaveChangesAsync(DBContext dbContext)
        {
            await _dataService.CommitAsync(dbContext);
        }
    }
}
