using Application.Core.Login.Models;
using Application.Core.Login.Modules;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Services
{
    public class DueyMasterModule : AbstractMasterModule
    {
        public DueyMasterModule(MasterServer server, ILogger<MasterModule> logger) : base(server, logger)
        {
        }

        public override async Task OnPlayerLogin(CharacterLiveObject obj)
        {
            await base.OnPlayerLogin(obj);

            await _server.DueyManager.SendDueyNotifyOnLogin(obj.Character.Id);
        }

        public override async Task OnPlayerLogoff(CharacterLiveObject obj)
        {
            await base.OnPlayerLogoff(obj);

            _server.DueyManager.PackageUnfreeze(obj.Character.Id);
        }
    }
}
