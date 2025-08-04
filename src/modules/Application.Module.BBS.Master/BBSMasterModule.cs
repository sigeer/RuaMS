using Application.Core.Login;
using Application.Core.Login.Events;
using Application.EF;
using Microsoft.Extensions.Logging;

namespace Application.Module.BBS.Master
{
    internal class BBSMasterModule : MasterModule
    {
        readonly BBSManager _manager;
        public BBSMasterModule(MasterServer server, ILogger<MasterModule> logger, BBSManager bBSManager) : base(server, logger)
        {
            _manager = bBSManager;
        }

        public override async Task SaveChangesAsync(DBContext dbContext)
        {
            await base.SaveChangesAsync(dbContext);
            await _manager.Commit(dbContext);
        }
    }
}
