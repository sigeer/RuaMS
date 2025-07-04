using Application.Core.Login;
using Application.Core.Login.Events;
using Microsoft.Extensions.Logging;

namespace Application.Module.Maker.Master
{
    internal class MakerMasterModule : MasterModule
    {
        public MakerMasterModule(MasterServer server, ILogger<MasterModule> logger) : base(server, logger)
        {
        }
    }
}
