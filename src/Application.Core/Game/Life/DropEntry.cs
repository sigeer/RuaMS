using server;
using tools;

namespace Application.Core.Game.Life
{
    /// <summary>
    /// 物品掉落
    /// </summary>
    public class DropEntry
    {
        private DropEntry() { }

        public static DropEntry Global(int continentId, int itemId, int chance, int itemMinCount, int itemMaxCount, short questId)
        {
            return new DropEntry
            {
                ContinentId = continentId,
                ItemId = itemId,
                Chance = chance,
                MinCount = itemMinCount,
                MaxCount = itemMaxCount,
                QuestId = questId,
                Type = DropType.GlobalDrop
            };
        }
        public static DropEntry MobDrop(int mobId, int itemId, int chance, int itemMinCount, int itemMaxCount, short questId)
        {
            return new DropEntry
            {
                DropperId = mobId,
                ItemId = itemId,
                Chance = chance,
                MinCount = itemMinCount,
                MaxCount = itemMaxCount,
                QuestId = questId,
                Type = DropType.MonsterDrop
            };
        }

        public static DropEntry ReactorDrop(int reactorId, int itemId, int chance, short questId)
        {
            return new DropEntry
            {
                DropperId = reactorId,
                ItemId = itemId,
                Chance = chance,
                MinCount = 1,
                MaxCount = 1,
                QuestId = questId,
                Type = DropType.ReactorDrop
            };
        }

        public static DropEntry ReactorDropMeso(int chance)
        {
            return new DropEntry
            {
                ItemId = 0,
                Chance = chance,
                MinCount = 1,
                MaxCount = 1,
                Type = DropType.ReactorDrop
            };
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
        public DropType Type { get; set; }
        public int DropperId { get; set; }

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

        public static void ClassifyDropEntries(List<DropEntry> allData,out List<DropEntry> item, out List<DropEntry> visibleQuest, out List<DropEntry> otherQuest, IPlayer chr)
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

    public enum DropType
    {
        GlobalDrop,
        MonsterDrop,
        ReactorDrop
    }
}
