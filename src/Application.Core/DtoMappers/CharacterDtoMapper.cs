using Application.Shared.Characters;
using Application.Shared.Items;
using AutoMapper;

namespace Application.Core.DtoMappers
{
    public class CharacterDtoMapper : Profile
    {
        public CharacterDtoMapper()
        {
            CreateMap<CharacterEntity, CharacterDto>()
                .ReverseMap();

            CreateMap<AccountEntity, AccountDto>();

            CreateMap<Inventoryitem, ItemDto>();
            CreateMap<Inventoryequipment, EquipDto>();

            CreateMap<DB_Storage, StorageDto>();

            CreateMap<Buddy, BuddyDto>();
            CreateMap<Keymap, KeyMapDto>();
            CreateMap<Monsterbook, MonsterbookDto>();
        }
    }
}
