namespace Application.Core.Channel.ServerData
{
    public class CharacterDiseaseManager
    {
        private Queue<IChannelClient> processDiseaseAnnouncePlayers = new();
        private Queue<IChannelClient> registeredDiseaseAnnouncePlayers = new();

        readonly WorldChannel _server;

        public CharacterDiseaseManager(WorldChannel server)
        {
            this._server = server;
        }

        public void registerAnnouncePlayerDiseases(IChannelClient c)
        {
            registeredDiseaseAnnouncePlayers.Enqueue(c);
        }


        public void HandleRun()
        {
            Queue<IChannelClient> processDiseaseAnnounceClients;
            processDiseaseAnnounceClients = new(processDiseaseAnnouncePlayers);
            processDiseaseAnnouncePlayers.Clear();

            while (processDiseaseAnnounceClients.TryDequeue(out var c))
            {
                if (c.OnlinedCharacter.isLoggedinWorld())
                {
                    c.OnlinedCharacter.announceDiseases();
                    c.OnlinedCharacter.collectDiseases();
                }
            }

            // this is to force the system to wait for at least one complete tick before releasing disease info for the registered clients
            while (registeredDiseaseAnnouncePlayers.TryDequeue(out var c))
            {
                processDiseaseAnnouncePlayers.Enqueue(c);
            }
        }

    }
}
