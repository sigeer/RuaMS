using Application.Utility.Tasks;

namespace Application.Module.ExpeditionBossLog.Master
{
    internal class BossLogTask : AbstractRunnable
    {
        readonly ExpeditionBossLogManager _manager;

        public BossLogTask(ExpeditionBossLogManager manager)
        {
            _manager = manager;
        }

        public override void HandleRun()
        {
            _manager.ResetBossLogTable();
        }
    }
}
