using Application.Shared.Characters;
using Application.Shared.Items;

namespace Application.Core.Datas
{
    public class CharacterViewObject
    {
        public CharacterDto Character { get; set; }
        public ItemDto[] InventoryItems { get; set; } = [];
    }
    public class CharacterValueObject : CharacterViewObject
    {
        public CashShopDto CashShop { get; set; }
        /// <summary>
        /// 账号下的其他玩家数据
        /// </summary>
        public CharacterLinkDto? Link { get; set; }
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
        /// 仅在登录时加载，但是不随着玩家保存而一起保存，单独保存
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
    }

}
