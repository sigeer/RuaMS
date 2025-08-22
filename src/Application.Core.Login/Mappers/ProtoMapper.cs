using Application.Core.Login.Datas;
using Application.Core.Login.Models;
using Application.Core.Login.Models.ChatRoom;
using Application.Core.Login.Models.Gachpons;
using Application.Core.Login.Models.Items;
using Application.Core.Login.ServerData;
using Application.Shared.Constants;
using Application.Shared.Items;
using Dto;
using Google.Protobuf.WellKnownTypes;
using Mapster;

namespace Application.Core.Login.Mappers
{
    /// <summary>
    /// Model &lt;=> proto
    /// </summary>
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

            config.NewConfig<DateTime, Timestamp>()
                .MapWith(src => Timestamp.FromDateTime(src.ToUniversalTime()));
            config.NewConfig<Timestamp, DateTime>()
                .MapWith(src => src.ToDateTime());

            config.NewConfig<CharacterModel, Dto.CharacterDto>()
                .TwoWays();

            config.NewConfig<FameLogModel, Dto.FameLogRecordDto>().TwoWays();
            config.NewConfig<PetIgnoreModel, Dto.PetIgnoreDto>().TwoWays();

            config.NewConfig<EquipModel, Dto.EquipDto>()
                .TwoWays();
            config.NewConfig<PetModel, Dto.PetDto>().TwoWays();
            config.NewConfig<RingSourceModel, ItemProto.RingDto>().TwoWays();
            config.NewConfig<ItemModel, Dto.ItemDto>().TwoWays();

            config.NewConfig<AccountCtrl, Dto.AccountCtrlDto>().TwoWays();
            config.NewConfig<AccountGame, Dto.AccountGameDto>().TwoWays();
            config.NewConfig<StorageModel, Dto.StorageDto>().TwoWays();

            config.NewConfig<MonsterbookModel, Dto.MonsterbookDto>().TwoWays();
            config.NewConfig<TrockLocationModel, Dto.TrockLocationDto>().TwoWays();
            config.NewConfig<AreaModel, Dto.AreaDto>().TwoWays();
            config.NewConfig<EventModel, Dto.EventDto>().TwoWays();

            config.NewConfig<QuestStatusModel, Dto.QuestStatusDto>().TwoWays();
            config.NewConfig<QuestProgressModel, Dto.QuestProgressDto>().TwoWays();
            config.NewConfig<MedalMapModel, Dto.MedalMapDto>().TwoWays();

            config.NewConfig<SkillModel, Dto.SkillDto>().TwoWays();
            config.NewConfig<SkillMacroModel, Dto.SkillMacroDto>().TwoWays();
            config.NewConfig<CoolDownModel, Dto.CoolDownDto>().TwoWays();

            config.NewConfig<KeyMapModel, Dto.KeyMapDto>().TwoWays();
            config.NewConfig<QuickSlotModel, Dto.QuickSlotDto>().TwoWays();

            config.NewConfig<SavedLocationModel, Dto.SavedLocationDto>().TwoWays();

            config.NewConfig<Dto.BuddyDto, BuddyModel>();

            config.NewConfig<PlayerBuffSaveModel, SyncProto.PlayerBuffDto>().TwoWays();
            config.NewConfig<BuffModel, Dto.BuffDto>().TwoWays();
            config.NewConfig<DiseaseModel, Dto.DiseaseDto>().TwoWays();

            config.NewConfig<CharacterLiveObject, SyncProto.PlayerGetterDto>()
                .Map(dest => dest.BuddyList, x => x.BuddyList.Values);
            config.NewConfig<CharacterLiveObject, Dto.PlayerViewDto>();

            config.NewConfig<CharacterLiveObject, TeamProto.TeamMemberDto>()
                .Map(dest => dest.Channel, x => x.Channel)
                .Map(dest => dest.Id, x => x.Character.Id)
                .Map(dest => dest.Name, x => x.Character.Name)
                .Map(dest => dest.Job, x => x.Character.JobId)
                .Map(dest => dest.Level, x => x.Character.Level)
                .Map(dest => dest.MapId, x => x.Character.Map);

            config.NewConfig<CharacterLiveObject, GuildProto.GuildMemberDto>()
                .Map(dest => dest.Channel, x => x.Channel)
                .Map(dest => dest.Id, x => x.Character.Id)
                .Map(dest => dest.Name, x => x.Character.Name)
                .Map(dest => dest.Job, x => x.Character.JobId)
                .Map(dest => dest.Level, x => x.Character.Level)
                .Map(dest => dest.GuildRank, x => x.Character.GuildRank)
                .Map(dest => dest.AllianceRank, x => x.Character.AllianceRank)
                .Map(dest => dest.GuildId, x => x.Character.GuildId);

            config.NewConfig<ChatRoomModel, Dto.ChatRoomDto>()
                .Map(dest => dest.RoomId, x => x.Id);

            config.NewConfig<GiftModel, ItemProto.GiftDto>();

            config.NewConfig<NewYearCardModel, Dto.NewYearCardDto>();
            config.NewConfig<PLifeModel, LifeProto.PLifeDto>()
                .TwoWays()
                .Map(dest => dest.LifeId, x => x.Life)
                .Map(dest => dest.MapId, x => x.Map);
            config.NewConfig<ItemQuantity, BaseProto.ItemQuantity>();

            config.NewConfig<ItemProto.PlayerShopItemDto, PlayerShopItemModel>().TwoWays();

            config.NewConfig<ItemModel, ItemModel>();
            config.NewConfig<PlayerShopItemModel, ItemModel>()
                .Map(dest => dest.Quantity, x => x.Bundles * x.Item.Quantity);

            config.NewConfig<NoteModel, Dto.NoteDto>();

            config.NewConfig<CallbackModel, Dto.RemoteCallDto>();
            config.NewConfig<CallbackParamModel, Dto.RemoteCallParamDto>();

            config.NewConfig<GachaponPoolModel, ItemProto.GachaponPoolDto>();
            config.NewConfig<GachaponPoolLevelChanceModel, ItemProto.GachaponPoolChanceDto>();
            config.NewConfig<GachaponPoolItemModel, ItemProto.GachaponPoolItemDto>();

            config.NewConfig<CdkItemModel, ItemProto.CdkRewordPackageDto>();
        }
    }
}
