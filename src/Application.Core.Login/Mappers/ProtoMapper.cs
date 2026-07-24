using Application.Core.Login.Models;
using Application.Core.Login.Models.ChatRoom;
using Application.Core.Login.Models.Gachpons;
using Application.Core.Login.Models.Items;
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


            config.NewConfig<RingSourceModel, ItemProto.RingDto>();
            config.NewConfig<ItemProto.RingDto, RingSourceModel>();

            config.NewConfig<AccountCtrl, AccountDto.AccountInfoProto>();


            config.NewConfig<CharacterLiveObject, SyncProto.PlayerGetterDto>();

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

            config.NewConfig<PLifeModel, LifeProto.PLifeDto>()
                .Map(dest => dest.LifeId, src => src.Life)
                .Map(dest => dest.MapId, src => src.Map);
            config.NewConfig<LifeProto.PLifeDto, PLifeModel>()
                .Map(dest => dest.Life, src => src.LifeId)
                .Map(dest => dest.Map, src => src.MapId);

            config.NewConfig<ItemQuantity, BaseProto.ItemQuantity>();


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
