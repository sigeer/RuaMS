using Application.Shared.Characters;
using Application.Shared.Dto;
using Application.Shared.Duey;
using Application.Shared.Items;
using AutoMapper;

namespace Application.Core.Login.Mappers
{
    /// <summary>
    /// 
    /// </summary>
    public class ProtoMapper : Profile
    {
        public ProtoMapper()
        {
            CreateMap<CharacterDto, Dto.CharacterDto>().ReverseMap();

            CreateMap<EquipDto, Dto.EquipDto>().ReverseMap();
            CreateMap<PetDto, Dto.PetDto>().ReverseMap();
            CreateMap<RingDto, Dto.RingDto>().ReverseMap();
            CreateMap<ItemDto, Dto.ItemDto>().ReverseMap();

            CreateMap<AccountDto, Dto.AccountDto>().ReverseMap();
            CreateMap<MonsterbookDto, Dto.MonsterbookDto>().ReverseMap();
            CreateMap<TrockLocationDto, Dto.TrockLocationDto>().ReverseMap();
            CreateMap<AreaDto, Dto.AreaDto>().ReverseMap();
            CreateMap<EventDto, Dto.EventDto>().ReverseMap();

            CreateMap<QuestStatusDto, Dto.QuestStatusDto>().ReverseMap();
            CreateMap<QuestProgressDto, Dto.QuestProgressDto>().ReverseMap();
            CreateMap<MedalMapDto, Dto.MedalMapDto>().ReverseMap();

            CreateMap<SkillDto, Dto.SkillDto>().ReverseMap();
            CreateMap<SkillMacroDto, Dto.SkillMacroDto>().ReverseMap();
            CreateMap<CoolDownDto, Dto.CoolDownDto>().ReverseMap();

            CreateMap<KeyMapDto, Dto.KeyMapDto>().ReverseMap();
            CreateMap<QuickSlotDto, Dto.QuickSlotDto>().ReverseMap();

            CreateMap<SavedLocationDto, Dto.SavedLocationDto>();
            CreateMap<StorageDto, Dto.StorageDto>();

            CreateMap<DropDto, Dto.DropDto>().ReverseMap(); ;

            CreateMap<DueyPackageDto, Dto.DueyPackageDto>().ReverseMap();
            CreateMap<NoteDto, Dto.NoteDto>().ReverseMap();
            CreateMap<ShopDto, Dto.ShopDto>()
                .ReverseMap();
            CreateMap<ShopItemDto, Dto.ShopItemDto>().ReverseMap();

            CreateMap<Dto.PlayerSaveDto, PlayerSaveDto>();
            CreateMap<CharacterValueObject, Dto.PlayerGetterDto>();
        }
    }
}
