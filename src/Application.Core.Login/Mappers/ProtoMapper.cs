using Application.Core.Game.Relation;
using Application.Core.Login.Models;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Login.Mappers
{
    /// <summary>
    /// Model &lt;=> proto
    /// </summary>
    public class ProtoMapper : Profile
    {
        public ProtoMapper()
        {
            CreateMap<Timestamp, DateTimeOffset?>()
                .ConvertUsing(src => src == null ? (DateTimeOffset?)null : src.ToDateTimeOffset());
            CreateMap<DateTimeOffset?, Timestamp>()
                .ConvertUsing(src => src.HasValue ? Timestamp.FromDateTimeOffset(src.Value) : null!);

            CreateMap<Timestamp, DateTimeOffset>()
                .ConvertUsing(src => src.ToDateTimeOffset());
            CreateMap<DateTimeOffset, Timestamp>()
                .ConvertUsing(src => Timestamp.FromDateTimeOffset(src));

            CreateMap<DateTime, Timestamp>().ConvertUsing(src => Timestamp.FromDateTime(src.ToUniversalTime()));
            CreateMap<Timestamp, DateTime>().ConvertUsing(src => src.ToDateTime());

            CreateMap<CharacterModel, Dto.CharacterDto>()
                .ReverseMap();

            CreateMap<PetIgnoreModel, Dto.PetIgnoreDto>().ReverseMap();

            CreateMap<EquipModel, Dto.EquipDto>().ReverseMap();
            CreateMap<PetModel, Dto.PetDto>().ReverseMap();
            CreateMap<RingModel, Dto.RingDto>().ReverseMap();
            CreateMap<ItemModel, Dto.ItemDto>().ReverseMap();

            CreateMap<AccountCtrl, Dto.AccountCtrlDto>().ReverseMap();
            CreateMap<AccountGame, Dto.AccountGameDto>().ReverseMap();
            CreateMap<StorageModel, Dto.StorageDto>().ReverseMap();

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

            CreateMap<SavedLocationModel, Dto.SavedLocationDto>().ReverseMap();

            CreateMap<DueyPackageModel, Dto.DueyPackageDto>()
                .ForMember(x => x.PackageId, src => src.MapFrom(x => x.Id));

            CreateMap<ShopModel, Dto.ShopDto>().ReverseMap();
            CreateMap<ShopItemModel, Dto.ShopItemDto>().ReverseMap();

            CreateMap<PlayerBuffSaveModel, Dto.PlayerBuffSaveDto>().ReverseMap();
            CreateMap<BuffModel, Dto.BuddyDto>().ReverseMap();
            CreateMap<DiseaseModel, Dto.DiseaseDto>().ReverseMap();

            CreateMap<CharacterLiveObject, Dto.PlayerGetterDto>();

            CreateMap<CharacterViewObject, Dto.TeamMemberDto>()
                .ForMember(dest => dest.Channel, src => src.MapFrom(x => x.Channel))
                .ForMember(dest => dest.Id, src => src.MapFrom(x => x.Character.Id))
                .ForMember(dest => dest.Name, src => src.MapFrom(x => x.Character.Name))
                .ForMember(dest => dest.Job, src => src.MapFrom(x => x.Character.JobId))
                .ForMember(dest => dest.Level, src => src.MapFrom(x => x.Character.Level));

            CreateMap<CharacterViewObject, Dto.GuildMemberDto>()
                .ForMember(dest => dest.Channel, src => src.MapFrom(x => x.Channel))
                .ForMember(dest => dest.Id, src => src.MapFrom(x => x.Character.Id))
                .ForMember(dest => dest.Name, src => src.MapFrom(x => x.Character.Name))
                .ForMember(dest => dest.Job, src => src.MapFrom(x => x.Character.JobId))
                .ForMember(dest => dest.Level, src => src.MapFrom(x => x.Character.Level))
                .ForMember(dest => dest.GuildRank, src => src.MapFrom(x => x.Character.GuildRank))
                .ForMember(dest => dest.AllianceRank, src => src.MapFrom(x => x.Character.AllianceRank))
                .ForMember(dest => dest.GuildId, src => src.MapFrom(x => x.Character.GuildId));
        }
    }
}
