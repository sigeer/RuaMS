using Application.Utility.Tasks;

namespace Application.Module.Duey.Master
{
    public class DueyTask : AbstractRunnable
    {
        readonly DueyManager _manager;

        public DueyTask(DueyManager manager)
        {
            _manager = manager;
        }

        public override void HandleRun()
        {
            _manager.RunDueyExpireSchedule();
        }
    }
}
