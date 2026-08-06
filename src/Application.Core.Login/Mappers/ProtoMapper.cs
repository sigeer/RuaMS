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

            config.NewConfig<AccountCtrl, ProtoModel.AccountInfoProto>();


            config.NewConfig<CharacterLiveObject, ProtoModel.PlayerGetterProto>();

            config.NewConfig<CharacterLiveObject, ProtoModel.PlayerViewProto>();

            config.NewConfig<CharacterLiveObject, ProtoModel.TeamMemberProto>()
                .Map(dest => dest.Channel, src => src.Channel)
                .Map(dest => dest.Id, src => src.Character.Id)
                .Map(dest => dest.Name, src => src.Character.Name)
                .Map(dest => dest.Job, src => src.Character.JobId)
                .Map(dest => dest.Level, src => src.Character.Level);

            config.NewConfig<CharacterLiveObject, ProtoModel.GuildMemberProto>()
                .Map(dest => dest.Channel, src => src.Channel)
                .Map(dest => dest.Id, src => src.Character.Id)
                .Map(dest => dest.Name, src => src.Character.Name)
                .Map(dest => dest.Job, src => src.Character.JobId)
                .Map(dest => dest.Level, src => src.Character.Level)
                .Map(dest => dest.GuildRank, src => src.Character.GuildRank)
                .Map(dest => dest.AllianceRank, src => src.Character.AllianceRank)
                .Map(dest => dest.GuildId, src => src.Character.GuildId);

            config.NewConfig<ChatRoomModel, ProtoModel.ChatRoomProto>()
                .Map(dest => dest.RoomId, src => src.Id)
                .Ignore(dest => dest.Members);


            config.NewConfig<ItemQuantity, ProtoModel.ItemQuantity>();


            config.NewConfig<CallbackModel, ProtoModel.RemoteCallProto>();
            config.NewConfig<CallbackParamModel, ProtoModel.RemoteCallParamProto>();

            config.NewConfig<CdkItemModel, ProtoModel.CdkRewordPackageProto>();
        }
    }
}
