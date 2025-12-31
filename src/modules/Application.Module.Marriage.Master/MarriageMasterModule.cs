using Application.Core.Login;
using Application.Core.Login.Models;
using Application.Core.Login.Modules;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Application.Module.Marriage.Master
{
    internal class MarriageMasterModule : AbstractMasterModule
    {
        readonly MarriageManager _marriageManager;
        public MarriageMasterModule(MasterServer server, ILogger<MasterModule> logger, MarriageManager marriageManager) : base(server, logger)
        {
            _marriageManager = marriageManager;
        }

        public override async Task OnPlayerLogin(CharacterLiveObject obj)
        {
            await base.OnPlayerLogin(obj);

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

        public override async Task OnPlayerLogoff(CharacterLiveObject obj)
        {
            await base.OnPlayerLogoff(obj);

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

        public override async Task OnPlayerMapChanged(CharacterLiveObject obj)
        {
            await base.OnPlayerMapChanged(obj);

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

        public override async Task OnPlayerEnterCashShop(CharacterLiveObject obj)
        {
            await base.OnPlayerEnterCashShop(obj);

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
