using Application.Core.Channel.Commands;
using Application.Core.Channel.ServerData;

namespace Application.Core.Channel.Tasks
{
    public class CharacterHpDecreaseTask : TaskBase
    {
        readonly WorldChannelServer _server;
        public CharacterHpDecreaseTask(WorldChannelServer server) : base(nameof(CharacterHpDecreaseTask)
            , TimeSpan.FromMilliseconds(YamlConfig.config.server.MAP_DAMAGE_OVERTIME_INTERVAL)
            , TimeSpan.FromMilliseconds(YamlConfig.config.server.MAP_DAMAGE_OVERTIME_INTERVAL))
        {
            _server = server;
        }
        protected override void HandleRun()
        {
            _server.Broadcast(w =>
            {
                w.CharacterHpDecreaseManager.HandleRun();
            });
        }

    }
}
