using Application.Shared.Characters;
using Application.Shared.Items;
using net.server.coordinator.session;

namespace Application.Core.Datas
{
    public class CharacterViewObject
    {
        public CharacterDto Character { get; set; }
        public ItemDto[] Items { get; set; }
    }
    public class CharacterValueObject: CharacterViewObject
    {
        public CharacterLinkDto? Link { get; set; }
        public AccountDto Account { get; set; }
        public MonsterbookDto[] MonsterBooks { get; set; }
        public PetIgnoreDto[] PetIgnores { get; set; }
        public TrockLocationDto[] TrockLocations { get; set; }
        public AreaDto[] Areas { get; set; }
        public EventDto[] Events { get; set; }

        public QuestStatusDto[] QuestStatuses { get; set; }
        public QuestProgressDto[] QuestProgresses { get; set; }
        public MedalMapDto[] MedalMaps { get; set; }
        public SkillDto[] Skills { get; set; }
        public SkillMacroDto[] SkillMacros { get; set; }
        public CoolDownDto[] CoolDowns { get; set; }
        public KeyMapDto[] KeyMaps { get; set; }
        public SavedLocationDto[] SavedLocations { get; set; }
        public RecentFameRecordDto FameRecord { get; set; }

        public QuickSlotDto? QuickSlot { get; set; }

        public StorageDto? StorageInfo { get; set; }
        public BuddyDto[] BuddyList { get; set; }
    }

    public class CharacterLoginInfo
    {
        public bool IsAccountOnlined { get; set; }
        public bool IsPlayerOnlined { get; set; }
        public Hwid Hwid { get; set; }
    }
}
