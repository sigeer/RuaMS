using Application.Shared.Characters;
using Application.Shared.Items;

namespace Application.Shared.Dto
{
    public class PlayerSaveDto
    {
        public CharacterDto Character { get; set; }
        public ItemDto[] InventoryItems { get; set; } = [];
        /// <summary>
        /// 与Account关联，即时保存
        /// </summary>
        public CashShopDto CashShop { get; set; }
        public MonsterbookDto[] MonsterBooks { get; set; }
        public PetIgnoreDto[] PetIgnores { get; set; }
        public TrockLocationDto[] TrockLocations { get; set; }
        public AreaDto[] Areas { get; set; }
        public EventDto[] Events { get; set; }

        public QuestStatusDto[] QuestStatuses { get; set; }

        public SkillDto[] Skills { get; set; }
        public SkillMacroDto[] SkillMacros { get; set; }
        public CoolDownDto[] CoolDowns { get; set; }
        public KeyMapDto[] KeyMaps { get; set; }
        public SavedLocationDto[] SavedLocations { get; set; }

        public QuickSlotDto? QuickSlot { get; set; }

        public StorageDto? StorageInfo { get; set; }
        public BuddyDto[] BuddyList { get; set; }
    }
}
