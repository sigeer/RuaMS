using Application.Utility.Configs;
using Application.Utility.Tasks;

namespace Application.Core.Login.Tasks
{
    internal class ServerTimeForceUpdateTask : ActorTask<MasterServer>
    {
        public ServerTimeForceUpdateTask(MasterServer actor) : base(actor, nameof(ServerTimeForceUpdateTask), TimeSpan.FromMilliseconds(YamlConfig.config.server.PURGING_INTERVAL))
        {
        }

        protected override void HandleRun()
        {
            _actor.ForceUpdateServerTime();
        }
    }
}
