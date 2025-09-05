namespace Application.Templates.Quest
{
    public sealed class QuestActTemplate
    {
        public int QuestId { get; set; }
        public QuestAct? StartAct { get; set; }
        public QuestAct? EndAct { get; set; }
    }

    public sealed class QuestAct
    {
        public int? Fame { get; set; }
        public int? Exp { get; set; }
        public int? Npc { get; set; }
        public int? Money { get; set; }
        public int? PetTameness { get; set; }
        public int? PetSpeed { get; set; }
        public int? PetSkill { get; set; }
        public int? NextQuest { get; set; }
        public int? BuffItemID { get; set; }
        public int[] MapID { get; set; }
        public int? LevelMin { get; set; }
        public int? LevelMax { get; set; }
        public int? Interval { get; set; }
        public string? Info { get; set; }

        public ActItem[] Items { get; set; }
        public ActSkill[] Skills { get; set; }
        public ActQuest[] Quests { get; set; }

        public QuestAct()
        {
            Items = Array.Empty<ActItem>();
            Skills = Array.Empty<ActSkill>();
            MapID = Array.Empty<int>();
            Quests = Array.Empty<ActQuest>();
        }

        public sealed class ActItem
        {
            public int ItemID { get; set; }
            public int Count { get; set; }
            public int Job { get; set; } = -1;

            public int Period { get; set; }
            public int? Prop { get; set; }
            public int Gender { get; set; } = 2;
        }

        public sealed class ActSkill
        {
            public int SkillID { get; set; }
            public int MasterLevel { get; set; }
            public int SkillLevel { get; set; }
            public int[] Job { get; set; } = new int[0];
        }

        public sealed class ActQuest
        {
            public int QuestId { get; set; }
            public int State { get; set; }
        }
    }
}
