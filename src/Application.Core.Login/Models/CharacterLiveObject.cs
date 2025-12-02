using Application.Core.Login.Servers;
using Application.Shared.NewYear;

namespace Application.Core.Login.Models
{
    public class CharacterViewObject
    {
        /// <summary>
        /// 仅在MasterServer使用，-1：商城，0：离线
        /// </summary>
        public int Channel { get; set; }
        /// <summary>
        /// 玩家所在的频道服务器节点
        /// </summary>
        public ChannelServerNode? ChannelNode { get; set; }
        public int ActualChannel => ChannelNode == null ? 0 : Channel;
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
        public TimerQuestModel[] RunningTimerQuests { get; set; } = [];

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
        public StorageModel GachaponStorage { get; set; }
    }

}
