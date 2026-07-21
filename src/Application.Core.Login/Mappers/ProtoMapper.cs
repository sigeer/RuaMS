using Application.Core.Login.Models;
using Application.Core.Login.Models.ChatRoom;
using Application.Core.Login.Models.Gachpons;
using Application.Core.Login.Models.Items;
using Application.Core.Login.ServerData;
using Application.EF.Entities;
using Application.Shared.Constants;
using Application.Shared.Items;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Login.Mappers
{
    public class ProtoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Timestamp, DateTimeOffset?>()
                .MapWith(src => src == null ? (DateTimeOffset?)null : src.ToDateTimeOffset());
            config.NewConfig<DateTimeOffset?, Timestamp>()
                .MapWith(src => src.HasValue ? Timestamp.FromDateTimeOffset(src.Value) : null!);

            config.NewConfig<Timestamp, DateTimeOffset>()
                .MapWith(src => src.ToDateTimeOffset());
            config.NewConfig<DateTimeOffset, Timestamp>()
                .MapWith(src => Timestamp.FromDateTimeOffset(src));

            config.NewConfig<DateTime, Timestamp>().MapWith(src => Timestamp.FromDateTime(src.ToUniversalTime()));
            config.NewConfig<Timestamp, DateTime>().MapWith(src => src.ToDateTime());

            config.NewConfig<CharacterModel, Dto.CharacterDto>();
            config.NewConfig<Dto.CharacterDto, CharacterModel>()
                .Ignore(dest => dest.Party)
                .Ignore(dest => dest.GuildId)
                .Ignore(dest => dest.GuildRank)
                .Ignore(dest => dest.AllianceRank)
                .Ignore(dest => dest.Jailexpire);

            config.NewConfig<FameLogModel, Dto.FameLogRecordDto>();
            config.NewConfig<Dto.FameLogRecordDto, FameLogModel>();

            config.NewConfig<PetIgnoreModel, Dto.PetIgnoreDto>();
            config.NewConfig<Dto.PetIgnoreDto, PetIgnoreModel>();

            config.NewConfig<EquipModel, Dto.EquipDto>();
            config.NewConfig<Dto.EquipDto, EquipModel>();

            config.NewConfig<PetModel, Dto.PetDto>();
            config.NewConfig<Dto.PetDto, PetModel>();

            config.NewConfig<RingSourceModel, ItemProto.RingDto>();
            config.NewConfig<ItemProto.RingDto, RingSourceModel>();

            config.NewConfig<ItemModel, Dto.ItemDto>();
            config.NewConfig<Dto.ItemDto, ItemModel>();

            config.NewConfig<AccountCtrl, AccountDto.AccountInfoProto>();

            config.NewConfig<AccountGame, Dto.AccountGameDto>();
            config.NewConfig<Dto.AccountGameDto, AccountGame>();

            config.NewConfig<StorageModel, Dto.StorageDto>();
            config.NewConfig<Dto.StorageDto, StorageModel>();

            config.NewConfig<MonsterbookModel, Dto.MonsterbookDto>();
            config.NewConfig<Dto.MonsterbookDto, MonsterbookModel>();

            config.NewConfig<TrockLocationModel, Dto.TrockLocationDto>();
            config.NewConfig<Dto.TrockLocationDto, TrockLocationModel>();

            config.NewConfig<AreaModel, Dto.AreaDto>();
            config.NewConfig<Dto.AreaDto, AreaModel>();

            config.NewConfig<EventModel, Dto.EventDto>();
            config.NewConfig<Dto.EventDto, EventModel>();

            config.NewConfig<QuestStatusModel, Dto.QuestStatusDto>();
            config.NewConfig<Dto.QuestStatusDto, QuestStatusModel>();

            config.NewConfig<QuestProgressModel, Dto.QuestProgressDto>();
            config.NewConfig<Dto.QuestProgressDto, QuestProgressModel>();

            config.NewConfig<MedalMapModel, Dto.MedalMapDto>();
            config.NewConfig<Dto.MedalMapDto, MedalMapModel>();

            config.NewConfig<SkillModel, Dto.SkillDto>();
            config.NewConfig<Dto.SkillDto, SkillModel>();

            config.NewConfig<SkillMacroModel, Dto.SkillMacroDto>();
            config.NewConfig<Dto.SkillMacroDto, SkillMacroModel>();

            config.NewConfig<CoolDownModel, Dto.CoolDownDto>();
            config.NewConfig<Dto.CoolDownDto, CoolDownModel>();

            config.NewConfig<KeyMapModel, Dto.KeyMapDto>();
            config.NewConfig<Dto.KeyMapDto, KeyMapModel>();

            config.NewConfig<QuickSlotModel, Dto.QuickSlotDto>();
            config.NewConfig<Dto.QuickSlotDto, QuickSlotModel>();

            config.NewConfig<SavedLocationModel, Dto.SavedLocationDto>();
            config.NewConfig<Dto.SavedLocationDto, SavedLocationModel>();

            config.NewConfig<CharacterLiveObject, SyncProto.PlayerGetterDto>()
                .Map(dest => dest.BuddyList, src => src.BuddyList.Values);
            config.NewConfig<CharacterLiveObject, Dto.PlayerViewDto>();

            config.NewConfig<CharacterLiveObject, TeamProto.TeamMemberDto>()
                .Map(dest => dest.Channel, src => src.Channel)
                .Map(dest => dest.Id, src => src.Character.Id)
                .Map(dest => dest.Name, src => src.Character.Name)
                .Map(dest => dest.Job, src => src.Character.JobId)
                .Map(dest => dest.Level, src => src.Character.Level);

            config.NewConfig<CharacterLiveObject, GuildProto.GuildMemberDto>()
                .Map(dest => dest.Channel, src => src.Channel)
                .Map(dest => dest.Id, src => src.Character.Id)
                .Map(dest => dest.Name, src => src.Character.Name)
                .Map(dest => dest.Job, src => src.Character.JobId)
                .Map(dest => dest.Level, src => src.Character.Level)
                .Map(dest => dest.GuildRank, src => src.Character.GuildRank)
                .Map(dest => dest.AllianceRank, src => src.Character.AllianceRank)
                .Map(dest => dest.GuildId, src => src.Character.GuildId);

            config.NewConfig<ChatRoomModel, Dto.ChatRoomDto>()
                .Map(dest => dest.RoomId, src => src.Id)
                .Ignore(dest => dest.Members);

            config.NewConfig<GiftModel, ItemProto.GiftDto>();

            config.NewConfig<NewYearCardModel, Dto.NewYearCardDto>();

            config.NewConfig<PLifeModel, LifeProto.PLifeDto>()
                .Map(dest => dest.LifeId, src => src.Life)
                .Map(dest => dest.MapId, src => src.Map);
            config.NewConfig<LifeProto.PLifeDto, PLifeModel>()
                .Map(dest => dest.Life, src => src.LifeId)
                .Map(dest => dest.Map, src => src.MapId);

            config.NewConfig<ItemQuantity, BaseProto.ItemQuantity>();

            config.NewConfig<ItemProto.PlayerShopItemDto, PlayerShopItemModel>();
            config.NewConfig<PlayerShopItemModel, ItemProto.PlayerShopItemDto>();

            config.NewConfig<PlayerShopItemModel, ItemModel>()
                .Map(dest => dest, src => src.Item)
                .Map(dest => dest.Quantity, src => src.Bundles * src.Item.Quantity) ;

            config.NewConfig<NoteModel, Dto.NoteDto>();

            config.NewConfig<CallbackModel, Dto.RemoteCallDto>();
            config.NewConfig<CallbackParamModel, Dto.RemoteCallParamDto>();

            config.NewConfig<GachaponPoolModel, ItemProto.GachaponPoolDto>();
            config.NewConfig<GachaponPoolLevelChanceModel, ItemProto.GachaponPoolChanceDto>();
            config.NewConfig<GachaponPoolItemModel, ItemProto.GachaponPoolItemDto>();

            config.NewConfig<CdkItemModel, ItemProto.CdkRewordPackageDto>();

            config.NewConfig<DueyPackageModel, DueyDto.DueyPackageDto>()
                .Map(dest => dest.PackageId, x => x.Id);
        }
    }
}
