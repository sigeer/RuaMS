using Application.Core.Channel.Commands;

namespace Application.Core.Channel.ServerData
{
    public class PetHungerManager
    {
        private Dictionary<int, int> activePets = new();
        private DateTimeOffset petUpdate;

        readonly WorldChannel _server;

        public PetHungerManager(WorldChannel server)
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

        public void unregisterPetHunger(Player chr, sbyte petSlot)
        {
            int key = getPetKey(chr, petSlot);

            activePets.Remove(key);
        }

        public void HandleRun()
        {
            Dictionary<int, int> deployedPets;

            petUpdate = DateTimeOffset.UtcNow;
            deployedPets = new(activePets);   // exception here found thanks to MedicOP

            foreach (var dp in deployedPets)
            {
                var chr = _server.getPlayerStorage().getCharacterById(dp.Key / 4);
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

                activePets.AddOrUpdate(dp.Key, dpVal);
            }
        }
    }
}
