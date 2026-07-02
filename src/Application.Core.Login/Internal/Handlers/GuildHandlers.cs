using System.Threading.Tasks;
using Application.Shared.Internal;
using Application.Shared.Message;
using Google.Protobuf;
using GuildProto;

namespace Application.Core.Login.Internal.Handlers
{
    internal class GuildHandlers
    {
        internal class GuildDisbandHandler : InternalSessionMasterHandler<GuildDisbandRequest>
        {
            public GuildDisbandHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.DisbandGuild;

            protected override Task HandleMessage(GuildDisbandRequest message)
            {
                return _server.GuildManager.DisbandGuild(message);
            }

            protected override GuildDisbandRequest Parse(ByteString content) => GuildDisbandRequest.Parser.ParseFrom(content);
        }
        internal class GuildMemberLeaveHandler : InternalSessionMasterHandler<GuildProto.LeaveGuildRequest>
        {
            public GuildMemberLeaveHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.LeaveGuild;

            protected override Task HandleMessage(LeaveGuildRequest message)
            {
                return _server.GuildManager.PlayerLeaveGuild(message);
            }

            protected override LeaveGuildRequest Parse(ByteString content) => LeaveGuildRequest.Parser.ParseFrom(content);
        }

        internal class GuildJoinHandler : InternalSessionMasterHandler<GuildProto.JoinGuildRequest>
        {
            public GuildJoinHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.JoinGuild;

            protected override Task HandleMessage(JoinGuildRequest message)
            {
                return _server.GuildManager.PlayerJoinGuild(message);
            }

            protected override JoinGuildRequest Parse(ByteString content) => JoinGuildRequest.Parser.ParseFrom(content);
        }

        internal class GuildExpelMemberHandler : InternalSessionMasterHandler<ExpelFromGuildRequest>
        {
            public GuildExpelMemberHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.ExpelGuildMember;

            protected override Task HandleMessage(ExpelFromGuildRequest message)
            {
                return _server.GuildManager.GuildExpelMember(message);
            }

            protected override ExpelFromGuildRequest Parse(ByteString content) => ExpelFromGuildRequest.Parser.ParseFrom(content);
        }

        internal class GuildGpUpdateHandler : InternalSessionMasterHandler<UpdateGuildGPRequest>
        {
            public GuildGpUpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateGuildGp;

            protected override Task HandleMessage(UpdateGuildGPRequest message)
            {
                return _server.GuildManager.UpdateGuildGPAsync(message);
            }

            protected override UpdateGuildGPRequest Parse(ByteString content) => UpdateGuildGPRequest.Parser.ParseFrom(content);
        }

        internal class GuildEmblemUpdateHandler : InternalSessionMasterHandler<GuildProto.UpdateGuildEmblemRequest>
        {
            public GuildEmblemUpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateGuildEmblem;

            protected override Task HandleMessage(UpdateGuildEmblemRequest message)
            {
                return _server.GuildManager.UpdateGuildEmblem(message);
            }

            protected override UpdateGuildEmblemRequest Parse(ByteString content) => UpdateGuildEmblemRequest.Parser.ParseFrom(content);
        }

        internal class GuildNoticeUpdateHandler : InternalSessionMasterHandler<UpdateGuildNoticeRequest>
        {
            public GuildNoticeUpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateGuildNotice;

            protected override Task HandleMessage(UpdateGuildNoticeRequest message)
            {
                return _server.GuildManager.UpdateGuildNotice(message);
            }

            protected override UpdateGuildNoticeRequest Parse(ByteString content) => UpdateGuildNoticeRequest.Parser.ParseFrom(content);
        }

        internal class GuildRankChangeHandler : InternalSessionMasterHandler<UpdateGuildMemberRankRequest>
        {
            public GuildRankChangeHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.ChangeGuildMemberRank;

            protected override Task HandleMessage(UpdateGuildMemberRankRequest message)
            {
                return _server.GuildManager.ChangePlayerGuildRank(message);
            }

            protected override UpdateGuildMemberRankRequest Parse(ByteString content) => UpdateGuildMemberRankRequest.Parser.ParseFrom(content);
        }

        internal class GuildRankTitleUpdateHandler : InternalSessionMasterHandler<UpdateGuildRankTitleRequest>
        {
            public GuildRankTitleUpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateGuildRankTitle;

            protected override Task HandleMessage(UpdateGuildRankTitleRequest message)
            {
                return _server.GuildManager.UpdateGuildRankTitle(message);
            }

            protected override UpdateGuildRankTitleRequest Parse(ByteString content) => UpdateGuildRankTitleRequest.Parser.ParseFrom(content);
        }

        internal class GuildUpdateCapacity : InternalSessionMasterHandler<UpdateGuildCapacityRequest>
        {
            public GuildUpdateCapacity(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateGuildCapacity;

            protected override Task HandleMessage(UpdateGuildCapacityRequest message)
            {
                return _server.GuildManager.IncreseGuildCapacity(message);
            }

            protected override UpdateGuildCapacityRequest Parse(ByteString content) => UpdateGuildCapacityRequest.Parser.ParseFrom(content);
        }

        internal class GuildDropMessage : InternalSessionMasterHandler<GuildDropMessageRequest>
        {
            public GuildDropMessage(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.DropGuildMessage;

            protected override Task HandleMessage(GuildDropMessageRequest message)
            {
                return _server.GuildManager.SendGuildMessage(message.GuildId, message.Type, message.Message);
            }

            protected override GuildDropMessageRequest Parse(ByteString content) => GuildDropMessageRequest.Parser.ParseFrom(content);
        }
    }
}
