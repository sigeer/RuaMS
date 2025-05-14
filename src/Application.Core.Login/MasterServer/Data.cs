using client;
using net.server;

namespace Application.Core.Login
{
    public partial class MasterServer
    {
        public PlayerBuffStorage BuffStorage { get; }

        public void StashCharacterBuff(int playerId, List<PlayerBuffValueHolder> allBuffs)
        {
            BuffStorage.addBuffsToStorage(playerId, allBuffs);
        }

        public void StashCharacterDisease(int playerId, Dictionary<Disease, DiseaseExpiration> allDiseases)
        {
            BuffStorage.addDiseasesToStorage(playerId, allDiseases);
        }

        private List<KeyValuePair<long, PlayerBuffValueHolder>> getLocalStartTimes(List<PlayerBuffValueHolder> lpbvl)
        {
            long curtime = getCurrentTime();
            return lpbvl.Select(x => new KeyValuePair<long, PlayerBuffValueHolder>(curtime - x.usedTime, x)).OrderBy(x => x.Key).ToList();
        }

        public List<KeyValuePair<long, PlayerBuffValueHolder>>? GetCharacterBuffs(int playerId)
        {
            var buffs = BuffStorage.getBuffsFromStorage(playerId);
            if (buffs != null)
            {
                return getLocalStartTimes(buffs);
            }
            return null;
        }

        public Dictionary<Disease, DiseaseExpiration>? GetCharacterDiseases(int playerId)
        {
            return BuffStorage.getDiseasesFromStorage(playerId); ;
        }
    }
}
