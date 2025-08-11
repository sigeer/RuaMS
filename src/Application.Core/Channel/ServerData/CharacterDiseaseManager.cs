namespace Application.Core.Channel.ServerData
{
    public class CharacterDiseaseManager : TaskBase
    {
        private object disLock = new object();
        private Queue<IChannelClient> processDiseaseAnnouncePlayers = new();
        private Queue<IChannelClient> registeredDiseaseAnnouncePlayers = new();

        readonly WorldChannelServer _server;

        public CharacterDiseaseManager(WorldChannelServer server) : base($"CharacterDiseaseController_{server.ServerName}",
            TimeSpan.FromMilliseconds(YamlConfig.config.server.UPDATE_INTERVAL),
            TimeSpan.FromMilliseconds(YamlConfig.config.server.UPDATE_INTERVAL))
        {
            this._server = server;
        }

        public void registerAnnouncePlayerDiseases(IChannelClient c)
        {
            Monitor.Enter(disLock);
            try
            {
                registeredDiseaseAnnouncePlayers.Enqueue(c);
            }
            finally
            {
                Monitor.Exit(disLock);
            }
        }


        protected override void HandleRun()
        {
            _server.UpdateServerTime();

            Queue<IChannelClient> processDiseaseAnnounceClients;
            Monitor.Enter(disLock);
            try
            {
                processDiseaseAnnounceClients = new(processDiseaseAnnouncePlayers);
                processDiseaseAnnouncePlayers.Clear();
            }
            finally
            {
                Monitor.Exit(disLock);
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

            Monitor.Enter(disLock);
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
                Monitor.Exit(disLock);
            }
        }

    }
}
