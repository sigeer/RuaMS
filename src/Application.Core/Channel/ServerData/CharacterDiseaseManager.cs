namespace Application.Core.Channel.ServerData
{
    public class CharacterDiseaseManager : TaskBase
    {
        private Lock disLock = new ();
        private Queue<IChannelClient> processDiseaseAnnouncePlayers = new();
        private Queue<IChannelClient> registeredDiseaseAnnouncePlayers = new();

        readonly WorldChannelServer _server;

        public CharacterDiseaseManager(WorldChannelServer server) : base($"ChannelServer:{server.ServerName}_{nameof(CharacterDiseaseManager)}",
            TimeSpan.FromMilliseconds(YamlConfig.config.server.UPDATE_INTERVAL),
            TimeSpan.FromMilliseconds(YamlConfig.config.server.UPDATE_INTERVAL))
        {
            this._server = server;
        }

        public void registerAnnouncePlayerDiseases(IChannelClient c)
        {
            disLock.Enter();
            try
            {
                registeredDiseaseAnnouncePlayers.Enqueue(c);
            }
            finally
            {
                disLock.Exit();
            }
        }


        protected override void HandleRun()
        {
            _server.UpdateServerTime();

            Queue<IChannelClient> processDiseaseAnnounceClients;
            disLock.Enter();
            try
            {
                processDiseaseAnnounceClients = new(processDiseaseAnnouncePlayers);
                processDiseaseAnnouncePlayers.Clear();
            }
            finally
            {
                disLock.Exit();
            }

            while (processDiseaseAnnounceClients.TryDequeue(out var c))
            {
                var player = c.Character;
                if (player != null && player.isLoggedinWorld())
                {
                    player.announceDiseases();
                    player.collectDiseases();
                }
            }

            disLock.Enter();
            try
            {
                // this is to force the system to wait for at least one complete tick before releasing disease info for the registered clients
                while (registeredDiseaseAnnouncePlayers.TryDequeue(out var c))
                {
                    processDiseaseAnnouncePlayers.Enqueue(c);
                }
            }
            finally
            {
                disLock.Exit();
            }
        }

    }
}
