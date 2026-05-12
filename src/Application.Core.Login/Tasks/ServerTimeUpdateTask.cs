using Application.Utility.Configs;
using Application.Utility.Tasks;

namespace Application.Core.Login.Tasks
{
    internal class ServerTimeUpdateTask : ActorTask<MasterServer>
    {
        public ServerTimeUpdateTask(MasterServer actor) : base(actor, nameof(ServerTimeUpdateTask), TimeSpan.FromMilliseconds(YamlConfig.config.server.MOB_STATUS_MONITOR_PROC))
        {
        }

        protected override void HandleRun()
        {
            _actor.UpdateServerTime(YamlConfig.config.server.MOB_STATUS_MONITOR_PROC);
        }
    }
}
