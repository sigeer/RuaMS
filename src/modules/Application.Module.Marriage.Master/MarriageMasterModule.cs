using Application.Core.Login;
using Application.Core.Login.Events;
using Application.Core.Login.Models;
using Application.EF;
using Application.Module.Marriage.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Module.Marriage.Master
{
    internal class MarriageMasterModule : MasterModule
    {
        readonly MarriageManager _marriageManager;
        public MarriageMasterModule(MasterServer server, ILogger<MasterModule> logger, MarriageManager marriageManager) : base(server, logger)
        {
            _marriageManager = marriageManager;
        }

        public override async Task IntializeDatabaseAsync(DBContext dbContext)
        {
            await _marriageManager.InitializeAsync(dbContext);
        }

        public override async Task SaveChangesAsync(DBContext dbContext)
        {
            await _marriageManager.Commit(dbContext);
        }

        public override void OnPlayerLoad(DBContext dbContext, CharacterModel chrModel)
        {
            var marriageData = dbContext.Marriages.AsNoTracking().Where(x => x.Husbandid == chrModel.Id || x.Wifeid == chrModel.Id)
                    .Where(x => x.Status != (int)MarriageStatusEnum.Divorced).FirstOrDefault();
            if (marriageData != null)
            {
                chrModel.EffectMarriageId = marriageData.Marriageid;
                chrModel.PartnerId = marriageData.Husbandid == chrModel.Id ? marriageData.Wifeid : marriageData.Husbandid;
            }
        }
    }
}
