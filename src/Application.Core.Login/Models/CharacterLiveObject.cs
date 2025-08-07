using Application.Shared.NewYear;

namespace Application.Core.Login.Models
{
    public class CharacterViewObject
    {
        /// <summary>
        /// 仅在MasterServer使用
        /// </summary>
        public int Channel { get; set; }
        public int ActualChannel { get; set; }
        public CharacterModel Character { get; set; }
        public ItemModel[] InventoryItems { get; set; } = [];
    }
    public class CharacterLiveObject : CharacterViewObject
    {
        public int[] WishItems { get; set; } = [];
        public MonsterbookModel[] MonsterBooks { get; set; }
        public PetIgnoreModel[] PetIgnores { get; set; }
        public TrockLocationModel[] TrockLocations { get; set; }
        public AreaModel[] Areas { get; set; }
        public EventModel[] Events { get; set; }

        public QuestStatusModel[] QuestStatuses { get; set; }

        public SkillModel[] Skills { get; set; }
        public SkillMacroModel[] SkillMacros { get; set; }
        /// <summary>
        /// 原逻辑不随着玩家保存而一起保存，仅断开连接时才保存，现简化成一起保存
        /// </summary>
        public CoolDownModel[] CoolDowns { get; set; }
        public KeyMapModel[] KeyMaps { get; set; }
        public SavedLocationModel[] SavedLocations { get; set; }
        public Dictionary<int, BuddyModel> BuddyList { get; set; }
        public NewYearCardModel[] NewYearCards { get; set; }
        public FameLogModel[] FameLogs { get; set; }

    }

}
