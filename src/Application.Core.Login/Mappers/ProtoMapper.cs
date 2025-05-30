using Application.Core.Login.Models;
using Application.Shared.Models;
using AutoMapper;

namespace Application.Core.Login.Mappers
{
    /// <summary>
    /// Model &lt;=> proto
    /// </summary>
    public class ProtoMapper : Profile
    {
        public ProtoMapper()
        {
            CreateMap<CharacterModel, Dto.CharacterDto>().ReverseMap();

            CreateMap<EquipModel, Dto.EquipDto>().ReverseMap();
            CreateMap<PetModel, Dto.PetDto>().ReverseMap();
            CreateMap<RingModel, Dto.RingDto>().ReverseMap();
            CreateMap<ItemModel, Dto.ItemDto>().ReverseMap();

            CreateMap<AccountCtrl, Dto.AccountCtrlDto>().ReverseMap();
            CreateMap<AccountGame, Dto.AccountGameDto>().ReverseMap();

            CreateMap<MonsterbookModel, Dto.MonsterbookDto>().ReverseMap();
            CreateMap<TrockLocationModel, Dto.TrockLocationDto>().ReverseMap();
            CreateMap<AreaModel, Dto.AreaDto>().ReverseMap();
            CreateMap<EventModel, Dto.EventDto>().ReverseMap();

            CreateMap<QuestStatusModel, Dto.QuestStatusDto>().ReverseMap();
            CreateMap<QuestProgressModel, Dto.QuestProgressDto>().ReverseMap();
            CreateMap<MedalMapModel, Dto.MedalMapDto>().ReverseMap();

            CreateMap<SkillModel, Dto.SkillDto>().ReverseMap();
            CreateMap<SkillMacroModel, Dto.SkillMacroDto>().ReverseMap();
            CreateMap<CoolDownModel, Dto.CoolDownDto>().ReverseMap();

            CreateMap<KeyMapModel, Dto.KeyMapDto>().ReverseMap();
            CreateMap<QuickSlotModel, Dto.QuickSlotDto>().ReverseMap();

            CreateMap<SavedLocationModel, Dto.SavedLocationDto>();
            CreateMap<StorageModel, Dto.StorageDto>();

            CreateMap<DueyPackageModel, Dto.DueyPackageDto>().ReverseMap();
            CreateMap<ShopModel, Dto.ShopDto>().ReverseMap();
            CreateMap<ShopItemModel, Dto.ShopItemDto>().ReverseMap();

            CreateMap<CharacterLiveObject, Dto.PlayerGetterDto>();
        }
    }
}
