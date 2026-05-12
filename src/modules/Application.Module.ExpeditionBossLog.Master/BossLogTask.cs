using Application.Core.Login;
using Application.Utility.Tasks;

namespace Application.Module.ExpeditionBossLog.Master
{
    internal class BossLogTask : ActorTask<MasterServer>
    {
        readonly ExpeditionBossLogManager _manager;

        public BossLogTask(MasterServer server, ExpeditionBossLogManager manager): base(server, nameof(BossLogTask), TimeSpan.FromDays(1))
        {
            _manager = manager;
        }

        protected override void HandleRun()
        {
            _manager.ResetBossLogTable();
        }
    }
}
