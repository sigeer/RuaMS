using Application.Core.Game.TheWorld;

namespace Application.Core.Game.Controllers
{
    public class CharacterDiseaseController : TimelyControllerBase
    {
        private object disLock = new object();
        private Queue<IChannelClient> processDiseaseAnnouncePlayers = new();
        private Queue<IChannelClient> registeredDiseaseAnnouncePlayers = new();

        readonly IWorldChannel worldChannel;

        public CharacterDiseaseController(IWorldChannel worldChannel) : base($"CharacterDiseaseController_{worldChannel.InstanceId}",
            TimeSpan.FromMilliseconds(YamlConfig.config.server.UPDATE_INTERVAL),
            TimeSpan.FromMilliseconds(YamlConfig.config.server.UPDATE_INTERVAL))
        {
            this.worldChannel = worldChannel;
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
            worldChannel.UpdateServerTime();

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
