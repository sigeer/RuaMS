using Application.Core.Login.Models;
using Application.Core.Login.Models.ChatRoom;
using Application.Core.Login.Models.Gachpons;
using Application.Core.Login.Models.Items;
using Application.Shared.Items;
using Application.Shared.NewYear;
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

            CreateMap<FameLogModel, Dto.FameLogRecordDto>().ReverseMap();
            CreateMap<PetIgnoreModel, Dto.PetIgnoreDto>().ReverseMap();

            CreateMap<EquipModel, Dto.EquipDto>()
                .ReverseMap();
            CreateMap<PetModel, Dto.PetDto>().ReverseMap();
            CreateMap<RingSourceModel, ItemProto.RingDto>()
                .ForMember(dest => dest.CharacterName1, src => src.MapFrom<RingCharacterName1ValueResolver>())
                .ForMember(dest => dest.CharacterName2, src => src.MapFrom<RingCharacterName2ValueResolver>())
                .ReverseMap();
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
            CreateMap<TimerQuestModel, SyncProto.PlayerTimerQuestDto>().ReverseMap();

            CreateMap<SkillModel, Dto.SkillDto>().ReverseMap();
            CreateMap<SkillMacroModel, Dto.SkillMacroDto>().ReverseMap();
            CreateMap<CoolDownModel, Dto.CoolDownDto>().ReverseMap();

            CreateMap<KeyMapModel, Dto.KeyMapDto>().ReverseMap();
            CreateMap<QuickSlotModel, Dto.QuickSlotDto>().ReverseMap();

            CreateMap<SavedLocationModel, Dto.SavedLocationDto>().ReverseMap();
            CreateMap<BuddyModel, Dto.BuddyDto>()
                .ConvertUsing<BuddyConverter>();

            CreateMap<Dto.BuddyDto, BuddyModel>();

            CreateMap<PlayerBuffSaveModel, SyncProto.PlayerBuffDto>().ReverseMap();
            CreateMap<BuffModel, Dto.BuffDto>().ReverseMap();
            CreateMap<DiseaseModel, Dto.DiseaseDto>().ReverseMap();

            CreateMap<CharacterLiveObject, SyncProto.PlayerGetterDto>()
                .ForMember(dest=> dest.BuddyList, src => src.MapFrom(x => x.BuddyList.Values));
            CreateMap<CharacterLiveObject, Dto.PlayerViewDto>();

            CreateMap<CharacterLiveObject, TeamProto.TeamMemberDto>()
                .ForMember(dest => dest.Channel, src => src.MapFrom(x => x.Channel))
                .ForMember(dest => dest.Id, src => src.MapFrom(x => x.Character.Id))
                .ForMember(dest => dest.Name, src => src.MapFrom(x => x.Character.Name))
                .ForMember(dest => dest.Job, src => src.MapFrom(x => x.Character.JobId))
                .ForMember(dest => dest.Level, src => src.MapFrom(x => x.Character.Level))
                .ForMember(dest => dest.MapId, src => src.MapFrom(x => x.Character.Map));

            CreateMap<CharacterLiveObject, GuildProto.GuildMemberDto>()
                .ForMember(dest => dest.Channel, src => src.MapFrom(x => x.Channel))
                .ForMember(dest => dest.Id, src => src.MapFrom(x => x.Character.Id))
                .ForMember(dest => dest.Name, src => src.MapFrom(x => x.Character.Name))
                .ForMember(dest => dest.Job, src => src.MapFrom(x => x.Character.JobId))
                .ForMember(dest => dest.Level, src => src.MapFrom(x => x.Character.Level))
                .ForMember(dest => dest.GuildRank, src => src.MapFrom(x => x.Character.GuildRank))
                .ForMember(dest => dest.AllianceRank, src => src.MapFrom(x => x.Character.AllianceRank))
                .ForMember(dest => dest.GuildId, src => src.MapFrom(x => x.Character.GuildId));

            CreateMap<ChatRoomModel, Dto.ChatRoomDto>()
                .ForMember(dest => dest.RoomId, src => src.MapFrom(x => x.Id))
                .ForMember(dest => dest.Members, src => src.Ignore());

            CreateMap<GiftModel, ItemProto.GiftDto>()
                .ForMember(dest => dest.FromName, src => src.MapFrom<GiftFromNameValueResolver>())
                .ForMember(dest => dest.ToName, src => src.MapFrom<GiftToNameValueResolver>());

            CreateMap<NewYearCardModel, Dto.NewYearCardDto>();
            CreateMap<PLifeModel, LifeProto.PLifeDto>()
                .ForMember(dest => dest.LifeId, src => src.MapFrom(x => x.Life))
                .ForMember(dest => dest.MapId, src => src.MapFrom(x => x.Map))
                .ReverseMap()
                .ForMember(dest => dest.Life, src => src.MapFrom(x => x.LifeId))
                .ForMember(dest => dest.Map, src => src.MapFrom(x => x.MapId));
            CreateMap<ItemQuantity, BaseProto.ItemQuantity>();

            CreateMap<ItemProto.PlayerShopItemDto, PlayerShopItemModel>().ReverseMap();

            CreateMap<ItemModel, ItemModel>(); 
            CreateMap<PlayerShopItemModel, ItemModel>()
                .IncludeMembers(src => src.Item)
                .ForMember(dest => dest.Quantity, src => src.MapFrom(x => x.Bundles * x.Item.Quantity));

            CreateMap<NoteModel, Dto.NoteDto>()
                .ForMember(dest => dest.From, src => src.MapFrom<NoteSenderNameValueResolver>())
                .ForMember(dest => dest.To, src => src.MapFrom<NoteReceiverNameValueResolver>());

            CreateMap<CallbackModel, Dto.RemoteCallDto>();
            CreateMap<CallbackParamModel, Dto.RemoteCallParamDto>();

            CreateMap<GachaponPoolModel, ItemProto.GachaponPoolDto>();
            CreateMap<GachaponPoolLevelChanceModel, ItemProto.GachaponPoolChanceDto>();
            CreateMap<GachaponPoolItemModel, ItemProto.GachaponPoolItemDto>();

            CreateMap<CdkItemModel, ItemProto.CdkRewordPackageDto>();
        }
    }
}
