using server;
using tools;

namespace Application.Core.Game.Life
{
    /// <summary>
    /// 物品掉落
    /// </summary>
    public class DropEntry
    {
        public DropEntry(int itemId, int chance) : this(itemId, chance, -1) { }
        public DropEntry(int itemId, int chance, short questId) : this(itemId, chance, 1, 1, questId) { }


        public DropEntry(int itemId, int chance, int min, int max, short questId) : this(null, itemId, chance, min, max, questId)
        {

        }

        /// <summary>
        /// Global
        /// </summary>
        /// <param name="continentId"></param>
        /// <param name="itemId"></param>
        /// <param name="chance"></param>
        /// <param name="itemMinCount"></param>
        /// <param name="itemMaxCount"></param>
        /// <param name="questId"></param>
        public DropEntry(int? continentId, int itemId, int chance, int itemMinCount, int itemMaxCount, short questId)
        {
            ItemId = itemId;
            Chance = chance;
            QuestId = questId;
            ContinentId = continentId;
            MinCount = itemMinCount;
            MaxCount = itemMaxCount;
        }
        public int ItemId { get; set; }
        /// <summary>
        /// 物品基础爆率，（非Reactor）最大值：1000000，每个物品单独计算是否掉落
        /// </summary>
        public int Chance { get; set; }
        public short QuestId { get; set; } = -1;

        public int? ContinentId { get; set; }
        /// <summary>
        /// 不会为0
        /// </summary>
        public int MinCount { get; set; }
        public int MaxCount { get; set; }

        public int GetRandomCount(int min, int max)
        {
            return Randomizer.rand(min, max);
        }

        public int GetRandomCount()
        {
            return Randomizer.rand(MinCount, MaxCount);
        }

        public int GetDropPosX(int dropType, int mobPos, int index)
        {
            var step = dropType == 3 ? 40 : 25;
            return mobPos + ((index % 2 == 0) ? (step * ((index + 1) / 2)) : -(step * (index / 2)));
        }

        public bool CanDrop(int chance)
        {
            return Randomizer.nextInt(1000000) < chance;
        }

        public bool CanDrop()
        {
            return Randomizer.nextInt(1000000) < Chance;
        }

        public static void ClassifyDropEntries(List<DropEntry> allData, List<DropEntry> item, List<DropEntry> visibleQuest, List<DropEntry> otherQuest, IPlayer chr)
        {
            ItemInformationProvider ii = ItemInformationProvider.getInstance();

            foreach (var mde in allData)
            {
                if (!ii.isQuestItem(mde.ItemId))
                {
                    item.Add(mde);
                }
                else
                {
                    if (chr.needQuestItem(mde.QuestId, mde.ItemId))
                    {
                        visibleQuest.Add(mde);
                    }
                    else
                    {
                        otherQuest.Add(mde);
                    }
                }
            }
        }
    }
}
