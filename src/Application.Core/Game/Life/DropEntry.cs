using Application.Core.Channel.DataProviders;

namespace Application.Core.Game.Life
{
    public class DropItemEntry
    {
        protected DropItemEntry() { }
        public DropItemEntry(int itemId, int chance, int minCount, int maxCount)
        {
            ItemId = itemId;
            Chance = chance;
            MinCount = minCount;
            MaxCount = maxCount;
        }

        public int ItemId { get; set; }
        /// <summary>
        /// 物品基础爆率，（非Reactor）最大值：1000000，每个物品单独计算是否掉落
        /// </summary>
        public int Chance { get; set; }
        public int MinCount { get; set; }
        public int MaxCount { get; set; }
    }
    /// <summary>
    /// 物品掉落
    /// </summary>
    public class DropEntry : DropItemEntry
    {
        private DropEntry() { }
        private DropEntry(int itemId, int chance, int itemMinCount, int itemMaxCount) : base(itemId, chance, itemMinCount, itemMaxCount) { }

        public static DropEntry Global(int continentId, int itemId, int chance, int itemMinCount, int itemMaxCount, short questId)
        {
            return new DropEntry(itemId, chance, itemMinCount, itemMaxCount)
            {
                ContinentId = continentId,
                QuestId = questId,
                Type = DropFromType.GlobalDrop
            };
        }
        public static DropEntry MobDrop(int mobId, int itemId, int chance, int itemMinCount, int itemMaxCount, short questId)
        {
            return new DropEntry(itemId, chance, itemMinCount, itemMaxCount)
            {
                DropperId = mobId,
                QuestId = questId,
                Type = DropFromType.MonsterDrop
            };
        }

        public static DropEntry ReactorDrop(int reactorId, int itemId, int chance, short questId)
        {
            return new DropEntry(itemId, chance, 1, 1)
            {
                DropperId = reactorId,
                QuestId = questId,
                Type = DropFromType.ReactorDrop
            };
        }

        public static DropEntry ReactorDropMeso(int chance)
        {
            return new DropEntry(0, chance, 1, 1)
            {
                Type = DropFromType.ReactorDrop
            };
        }

        public short QuestId { get; set; } = -1;
        public int? ContinentId { get; set; }
        public DropFromType Type { get; set; }
        public int DropperId { get; set; }

        public int GetRandomCount(int min, int max)
        {
            return Randomizer.rand(min, max);
        }

        public int GetRandomCount()
        {
            return Randomizer.rand(MinCount, MaxCount);
        }

        public int GetDropPosX(DropType dropType, int mobPos, int index)
        {
            var step = dropType == DropType.FreeForAll_Explosive ? 40 : 25;
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

        public static void ClassifyDropEntries(List<DropEntry> allData, out List<DropEntry> item, out List<DropEntry> visibleQuest, out List<DropEntry> otherQuest, Player chr)
        {
            item = [];
            visibleQuest = [];
            otherQuest = [];

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
