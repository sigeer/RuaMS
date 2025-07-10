using Application.Core.Login;
using Application.Core.Login.Events;
using Application.EF;
using Microsoft.Extensions.Logging;

namespace Application.Module.PlayerNPC.Master
{
    internal class PlayerNPCMasterModule : MasterModule
    {
        readonly PlayerNPCManager _manager;
        public PlayerNPCMasterModule(MasterServer server, ILogger<MasterModule> logger, PlayerNPCManager manager) : base(server, logger)
        {
            _manager = manager;
        }

        public override async Task IntializeDatabaseAsync(DBContext dbContext)
        {
            await base.IntializeDatabaseAsync(dbContext);
            await _manager.Initialize(dbContext);
        }

        public override async Task SaveChangesAsync(DBContext dbContext)
        {
            await base.SaveChangesAsync(dbContext);
            await _manager.Commit(dbContext);
        }
    }
}
