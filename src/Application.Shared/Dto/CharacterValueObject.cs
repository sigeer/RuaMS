using Application.Shared.Characters;
using Application.Shared.Items;

namespace Application.Shared.Dto
{
    public class CharacterViewObject
    {
        public CharacterDto Character { get; set; }
        public ItemDto[] InventoryItems { get; set; } = [];
    }
    public class CharacterValueObject : CharacterViewObject
    {
        /// <summary>
        /// 与Account关联，即时保存
        /// </summary>
        public CashShopDto CashShop { get; set; }
        /// <summary>
        /// 账号下的其他玩家数据，仅读取用
        /// </summary>
        public CharacterLinkDto? Link { get; set; }
        /// <summary>
        /// 避免同一账号下的不同角色读取结果不同，采用即时保存
        /// </summary>
        public AccountDto Account { get; set; }
        public MonsterbookDto[] MonsterBooks { get; set; }
        public PetIgnoreDto[] PetIgnores { get; set; }
        public TrockLocationDto[] TrockLocations { get; set; }
        public AreaDto[] Areas { get; set; }
        public EventDto[] Events { get; set; }

        public QuestStatusDto[] QuestStatuses { get; set; }

        public SkillDto[] Skills { get; set; }
        public SkillMacroDto[] SkillMacros { get; set; }
        /// <summary>
        /// 原逻辑不随着玩家保存而一起保存，仅断开连接时才保存，现简化成一起保存
        /// </summary>
        public CoolDownDto[] CoolDowns { get; set; }
        public KeyMapDto[] KeyMaps { get; set; }
        public SavedLocationDto[] SavedLocations { get; set; }
        /// <summary>
        /// 单独保存
        /// </summary>
        public RecentFameRecordDto FameRecord { get; set; }

        public QuickSlotDto? QuickSlot { get; set; }

        public StorageDto? StorageInfo { get; set; }
        public BuddyDto[] BuddyList { get; set; }
        /// <summary>
        /// 仅在登录时返回的额外字段，不参与保存
        /// </summary>
        public LoginInfo LoginInfo { get; set; } = null!;
        /// <summary>
        /// 仅在MasterServer使用
        /// </summary>
        public int Channel { get; set; }
    }

}
