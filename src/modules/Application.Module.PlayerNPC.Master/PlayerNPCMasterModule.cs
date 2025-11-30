using Application.Core.Login;
using Application.Core.Login.Modules;
using Microsoft.Extensions.Logging;

namespace Application.Module.PlayerNPC.Master
{
    internal class PlayerNPCMasterModule : AbstractMasterModule
    {
        readonly PlayerNPCManager _manager;
        public PlayerNPCMasterModule(MasterServer server, ILogger<MasterModule> logger, PlayerNPCManager manager) : base(server, logger)
        {
            _manager = manager;
        }
    }
}
