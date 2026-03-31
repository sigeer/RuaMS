namespace Application.Core.Channel.Tasks
{
    public class CharacterDiseaseTask : TaskBase
    {
        readonly WorldChannelServer _server;

        public CharacterDiseaseTask(WorldChannelServer server) : base(nameof(CharacterDiseaseTask),
            TimeSpan.FromMilliseconds(YamlConfig.config.server.UPDATE_INTERVAL),
            TimeSpan.FromMilliseconds(YamlConfig.config.server.UPDATE_INTERVAL))
        {
            this._server = server;
        }

        protected override void HandleRun()
        {
            _server.UpdateServerTime();
        }

    }
}
