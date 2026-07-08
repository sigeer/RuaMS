using Application.Core.Scripting.Events;
using tools;

namespace Application.Core.scripting.Events.Instances
{
    public sealed class AriantEventInstanceManager : BehindPartyQuestEventInstanceManager
    {
        private int lostShards = 0;

        private Dictionary<Player, int> score;
        private Dictionary<Player, int> rewardTier;
        private bool scoreDirty = false;

        public int MaxCount { get; set; }
        public AriantEventInstanceManager(BehindPartyQuestEventManager em, string instanceName) : base(em, instanceName)
        {
            score = new();
            rewardTier = new();

            MaxCount = EventManager.MaxCount;

            exclusiveItems = [4031868, 2270002, 2100067];
        }

        public int getAriantRewardTier(Player chr)
        {
            return rewardTier.GetValueOrDefault(chr);
        }
        public int getAriantScore(Player chr)
        {
            return score.GetValueOrDefault(chr);
        }

        public void updateAriantScore(Player chr, int points)
        {
            if (IsEventInProgress())
            {
                score.AddOrUpdate(chr, points);
                scoreDirty = true;
            }
        }

        public async Task broadcastAriantScoreUpdate()
        {
            if (scoreDirty)
            {
                foreach (Player chr in getPlayers())
                {
                    await chr.SendPacket(PacketCreator.updateAriantPQRanking(score));
                }
                scoreDirty = false;
            }
        }

        public void addLostShards(int quantity)
        {
            lostShards += quantity;
        }

        private static bool isUnfairMatch(int winnerScore, int secondScore, int lostShardsScore, List<int> runnerupsScore)
        {
            if (winnerScore <= 0)
            {
                return false;
            }

            double runnerupsScoreSum = 0;
            foreach (int i in runnerupsScore)
            {
                runnerupsScoreSum += i;
            }

            runnerupsScoreSum += lostShardsScore;
            secondScore += lostShardsScore;

            double matchRes = runnerupsScoreSum / winnerScore;
            double runnerupRes = ((double)secondScore) / winnerScore;

            return matchRes < 0.81770726891980117713114871015349 && (runnerupsScoreSum < 7 || runnerupRes < 0.5929);
        }

        public async Task distributeAriantPoints()
        {
            int firstTop = -1, secondTop = -1;
            Player? winner = null;
            List<int> runnerups = new();

            foreach (var e in score)
            {
                int s = e.Value;
                if (s > firstTop)
                {
                    secondTop = firstTop;
                    firstTop = s;
                    winner = e.Key;
                }
                else if (s > secondTop)
                {
                    secondTop = s;
                }

                runnerups.Add(s);
                rewardTier.AddOrUpdate(e.Key, (int)Math.Floor((double)s / 10));
            }

            runnerups.Remove(firstTop);
            if (isUnfairMatch(firstTop, secondTop, ((await getInstanceMap(EventManager.EntryMap))?.getDroppedItemsCountById(ItemId.ARPQ_SPIRIT_JEWEL) ?? 0) + lostShards, runnerups))
            {
                rewardTier.AddOrUpdate(winner!, 1);
            }
        }
    }
}
