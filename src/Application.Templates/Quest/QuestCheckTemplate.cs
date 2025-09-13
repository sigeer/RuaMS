namespace Application.Templates.Quest
{
    public sealed class QuestCheckTemplate
    {
        public QuestCheckTemplate(int questId)
        {
            QuestId = questId;
        }

        public int QuestId { get; set; }
        public QuestDemand? StartDemand { get; set; }
        public QuestDemand? EndDemand { get; set; }
    }

    public sealed class QuestDemand
    {
        public int? Npc { get; set; }
        public int? LevelMin { get; set; }
        public int? LevelMax { get; set; }
        public int? Interval { get; set; }
        /// <summary>
        /// yyyyMMddHH
        /// </summary>
        public string? Start { get; set; }
        /// <summary>
        /// yyyyMMddHH
        /// </summary>
        public string? End { get; set; }
        public string? StartScript { get; set; }
        public string? EndScript { get; set; }
        public QuestRecord[] DemandQuest { get; set; }
        public MobInfo[] DemandMob { get; set; }
        public ItemInfo[] DemandItem { get; set; }
        public SkillInfo[] DemandSkill { get; set; }
        public int[] Job { get; set; }
        public PetInfo[] Pet { get; set; }
        public int[] FieldEnter { get; set; }
        public bool NormalAutoStart { get; set; }
        public int? PetTamenessMin { get; set; }
        public int? InfoNumber { get; set; }
        public QuestInfoEx[] InfoEx { get; set; }
        public int? QuestComplete { get; set; }
        public bool DayByDay { get; set; }
        public int? Buff { get; set; }
        public int? ExceptBuff { get; set; }
        public int? Meso { get; set; }
        public int? MinMonsterBook { get; set; }
        public QuestDemand()
        {
            DemandQuest = Array.Empty<QuestRecord>();
            DemandItem = Array.Empty<ItemInfo>();
            DemandSkill = Array.Empty<SkillInfo>();
            DemandMob = Array.Empty<MobInfo>();
            Job = Array.Empty<int>();
            Pet = Array.Empty<PetInfo>();
            FieldEnter = Array.Empty<int>();
            InfoEx = Array.Empty<QuestInfoEx>();
        }


        public sealed class QuestRecord
        {
            public int QuestID { get; set; }
            public int State { get; set; }
        }

        public sealed class ItemInfo
        {
            public int ItemID { get; set; }
            public int Count { get; set; }
        }

        public sealed class MobInfo
        {
            public int MobID { get; set; }
            public int Count { get; set; }
        }

        public sealed class SkillInfo
        {
            public int SkillID { get; set; }
            public int Acquire { get; set; }
        }

        public sealed class PetInfo
        {
            public int PetID { get; set; }
        }

        public sealed class QuestInfoEx
        {
            public string Value { get; set; }
            public int Cond { get; set; }
        }
    }
}
