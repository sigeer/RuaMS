using Application.Core.Login;
using Application.Core.Login.Modules;
using Microsoft.Extensions.Logging;

namespace Application.Module.Maker.Master
{
    internal class MakerMasterModule : AbstractMasterModule
    {
        public MakerMasterModule(MasterServer server, ILogger<MasterModule> logger) : base(server, logger)
        {
        }
    }
}
