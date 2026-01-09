namespace Application.Core.Channel.ServerData
{
    public class PetHungerManager : TaskBase
    {
        private Lock activePetsLock = new ();
        private Dictionary<int, int> activePets = new();
        private DateTimeOffset petUpdate;

        readonly WorldChannelServer _server;

        public PetHungerManager(WorldChannelServer server) : base($"ChannelServer:{server.ServerName}_{nameof(PetHungerManager)}", TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1))
        {
            petUpdate = DateTimeOffset.UtcNow;
            this._server = server;
        }

        private static int getPetKey(Player chr, sbyte petSlot)
        {
            // assuming max 3 pets
            return (chr.getId() << 2) + petSlot;
        }


        public void registerPetHunger(Player chr, sbyte petSlot)
        {
            if (chr.isGM() && YamlConfig.config.server.GM_PETS_NEVER_HUNGRY || YamlConfig.config.server.PETS_NEVER_HUNGRY)
            {
                return;
            }

            int key = getPetKey(chr, petSlot);

            activePetsLock.Enter();
            try
            {
                int initProc;
                if (DateTimeOffset.UtcNow - petUpdate > TimeSpan.FromSeconds(55))
                {
                    initProc = YamlConfig.config.server.PET_EXHAUST_COUNT - 2;
                }
                else
                {
                    initProc = YamlConfig.config.server.PET_EXHAUST_COUNT - 1;
                }

                activePets.AddOrUpdate(key, initProc);
            }
            finally
            {
                activePetsLock.Exit();
            }
        }

        public void unregisterPetHunger(Player chr, sbyte petSlot)
        {
            int key = getPetKey(chr, petSlot);

            activePetsLock.Enter();
            try
            {
                activePets.Remove(key);
            }
            finally
            {
                activePetsLock.Exit();
            }
        }

        protected override void HandleRun()
        {
            Dictionary<int, int> deployedPets;

            activePetsLock.Enter();
            try
            {
                petUpdate = DateTimeOffset.UtcNow;
                deployedPets = new(activePets);   // exception here found thanks to MedicOP
            }
            finally
            {
                activePetsLock.Exit();
            }

            foreach (var dp in deployedPets)
            {
                var chr = _server.FindPlayerById(dp.Key / 4);
                if (chr == null || !chr.isLoggedinWorld())
                {
                    continue;
                }

                int dpVal = dp.Value + 1;
                if (dpVal == YamlConfig.config.server.PET_EXHAUST_COUNT)
                {
                    chr.runFullnessSchedule(dp.Key % 4);
                    dpVal = 0;
                }

                activePetsLock.Enter();
                try
                {
                    activePets.AddOrUpdate(dp.Key, dpVal);
                }
                finally
                {
                    activePetsLock.Exit();
                }
            }
        }
    }
}
