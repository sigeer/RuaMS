using Application.Core.Game.TheWorld;
using System.Collections.Concurrent;
using tools.packets;

namespace Application.Core.Gameplay.WorldEvents
{
    public class FishingWorldInstance
    {
        private ConcurrentDictionary<IPlayer, int> fishingAttempters = new();

        public FishingWorldInstance(IWorld worldServer)
        {
            WorldServer = worldServer;
        }

        public IWorld WorldServer { get; set; }
        public bool RegisterFisherPlayer(IPlayer chr, int baitLevel)
        {
            return fishingAttempters.TryAdd(chr, baitLevel);
        }

        public int UnregisterFisherPlayer(IPlayer chr)
        {
            if (fishingAttempters.TryRemove(chr, out var baitLevel))
            {
                return baitLevel;
            }
            else
            {
                return 0;
            }
        }

        public void RunCheckFishingSchedule()
        {
            double[] fishingLikelihoods = Fishing.fetchFishingLikelihood();
            double yearLikelihood = fishingLikelihoods[0], timeLikelihood = fishingLikelihoods[1];

            if (fishingAttempters.Count > 0)
            {
                List<IPlayer> fishingAttemptersList = fishingAttempters.Keys.ToList();
                foreach (IPlayer chr in fishingAttemptersList)
                {
                    int baitLevel = UnregisterFisherPlayer(chr);
                    Fishing.doFishing(chr, baitLevel, yearLikelihood, timeLikelihood);
                }
            }
        }
    }
}
