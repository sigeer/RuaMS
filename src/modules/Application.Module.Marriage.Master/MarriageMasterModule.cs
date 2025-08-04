using Application.Core.Login;
using Application.Core.Login.Events;
using Application.Core.Login.Models;
using Application.EF;
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

        public override async Task SaveChangesAsync(DBContext dbContext)
        {
            await _marriageManager.Commit(dbContext);
        }

        public override void OnPlayerLogin(CharacterLiveObject obj)
        {
            base.OnPlayerLogin(obj);

            var info = _marriageManager.GetEffectMarriageModel(obj.Character.Id);
            if (info != null)
            {
                var partnerId = info.GetPartnerId(obj.Character.Id);
                var partner = _server.CharacterManager.FindPlayerById(partnerId);
                if (partner != null && partner.Channel > 0)
                {
                    _marriageManager.NotifyPartner(partner.Character.Id, obj.Character.Id, obj.Character.Map);
                    _marriageManager.NotifyPartner(obj.Character.Id, partner.Character.Id, partner.Character.Map);
                }
            }
        }

        public override void OnPlayerLogoff(CharacterLiveObject obj)
        {
            base.OnPlayerLogoff(obj);

            var info = _marriageManager.GetEffectMarriageModel(obj.Character.Id);
            if (info != null)
            {
                var partnerId = info.GetPartnerId(obj.Character.Id);
                var partner = _server.CharacterManager.FindPlayerById(partnerId);
                if (partner != null && partner.Channel > 0)
                {
                    _marriageManager.NotifyPartner(partner.Character.Id, obj.Character.Id, obj.Character.Map);
                }
            }
        }

        public override void OnPlayerMapChanged(CharacterLiveObject obj)
        {
            base.OnPlayerMapChanged(obj);

            var info = _marriageManager.GetEffectMarriageModel(obj.Character.Id);
            if (info != null)
            {
                var partnerId = info.GetPartnerId(obj.Character.Id);
                var partner = _server.CharacterManager.FindPlayerById(partnerId);
                if (partner != null && partner.Channel > 0)
                {
                    _marriageManager.NotifyPartner(partner.Character.Id, obj.Character.Id, obj.Character.Map);
                }
            }
        }

        public override void OnPlayerEnterCashShop(CharacterLiveObject obj)
        {
            base.OnPlayerEnterCashShop(obj);

            var info = _marriageManager.GetEffectMarriageModel(obj.Character.Id);
            if (info != null)
            {
                var partnerId = info.GetPartnerId(obj.Character.Id);
                var partner = _server.CharacterManager.FindPlayerById(partnerId);
                if (partner != null && partner.Channel > 0)
                {
                    _marriageManager.NotifyPartner(partner.Character.Id, obj.Character.Id, obj.Character.Map);
                }
            }
        }
    }
}
