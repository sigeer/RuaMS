using Application.Core.Channel.Commands;

namespace Application.Core.Channel.Tasks
{
    public class CharacterHpDecreaseTask : TaskBase
    {
        readonly WorldChannelServer _server;
        public CharacterHpDecreaseTask(WorldChannelServer server) : base($"{server.InstanceName}_{nameof(CharacterHpDecreaseTask)}"
            , TimeSpan.FromMilliseconds(YamlConfig.config.server.MAP_DAMAGE_OVERTIME_INTERVAL)
            , TimeSpan.FromMilliseconds(YamlConfig.config.server.MAP_DAMAGE_OVERTIME_INTERVAL))
        {
            _server = server;
        }
        protected override void HandleRun()
        {
            _server.PushChannelCommand(new InvokeChannelMapDamageCommand());
        }

    }
}
