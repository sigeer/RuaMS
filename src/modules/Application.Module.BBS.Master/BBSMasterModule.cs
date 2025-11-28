using Application.Core.Login;
using Application.Core.Login.Modules;
using Microsoft.Extensions.Logging;

namespace Application.Module.BBS.Master
{
    internal class BBSMasterModule : AbstractMasterModule
    {
        readonly BBSManager _manager;
        public BBSMasterModule(MasterServer server, ILogger<MasterModule> logger, BBSManager bBSManager) : base(server, logger)
        {
            _manager = bBSManager;
        }
    }
}
