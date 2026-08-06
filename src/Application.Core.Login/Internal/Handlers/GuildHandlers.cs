using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Login.Internal.Handlers
{
    internal class GuildHandlers
    {
        internal class GuildDisbandHandler : InternalSessionMasterHandler<ProtoService.GuildDisbandRequest>
        {
            public GuildDisbandHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.DisbandGuild;

            protected override Task HandleMessage(ProtoService.GuildDisbandRequest message)
            {
                return _server.GuildManager.DisbandGuild(message);
            }

            protected override ProtoService.GuildDisbandRequest Parse(ByteString content) => ProtoService.GuildDisbandRequest.Parser.ParseFrom(content);
        }
        internal class GuildMemberLeaveHandler : InternalSessionMasterHandler<ProtoService.LeaveGuildRequest>
        {
            public GuildMemberLeaveHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.LeaveGuild;

            protected override Task HandleMessage(ProtoService.LeaveGuildRequest message)
            {
                return _server.GuildManager.PlayerLeaveGuild(message);
            }

            protected override ProtoService.LeaveGuildRequest Parse(ByteString content) => ProtoService.LeaveGuildRequest.Parser.ParseFrom(content);
        }

        internal class GuildJoinHandler : InternalSessionMasterHandler<ProtoService.JoinGuildRequest>
        {
            public GuildJoinHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.JoinGuild;

            protected override Task HandleMessage(ProtoService.JoinGuildRequest message)
            {
                return _server.GuildManager.PlayerJoinGuild(message);
            }

            protected override ProtoService.JoinGuildRequest Parse(ByteString content) => ProtoService.JoinGuildRequest.Parser.ParseFrom(content);
        }

        internal class GuildExpelMemberHandler : InternalSessionMasterHandler<ProtoService.ExpelFromGuildRequest>
        {
            public GuildExpelMemberHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.ExpelGuildMember;

            protected override Task HandleMessage(ProtoService.ExpelFromGuildRequest message)
            {
                return _server.GuildManager.GuildExpelMember(message);
            }

            protected override ProtoService.ExpelFromGuildRequest Parse(ByteString content) => ProtoService.ExpelFromGuildRequest.Parser.ParseFrom(content);
        }

        internal class GuildGpUpdateHandler : InternalSessionMasterHandler<ProtoService.UpdateGuildGPRequest>
        {
            public GuildGpUpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateGuildGp;

            protected override Task HandleMessage(ProtoService.UpdateGuildGPRequest message)
            {
                return _server.GuildManager.UpdateGuildGPAsync(message);
            }

            protected override ProtoService.UpdateGuildGPRequest Parse(ByteString content) => ProtoService.UpdateGuildGPRequest.Parser.ParseFrom(content);
        }

        internal class GuildEmblemUpdateHandler : InternalSessionMasterHandler<ProtoService.UpdateGuildEmblemRequest>
        {
            public GuildEmblemUpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateGuildEmblem;

            protected override Task HandleMessage(ProtoService.UpdateGuildEmblemRequest message)
            {
                return _server.GuildManager.UpdateGuildEmblem(message);
            }

            protected override ProtoService.UpdateGuildEmblemRequest Parse(ByteString content) => ProtoService.UpdateGuildEmblemRequest.Parser.ParseFrom(content);
        }

        internal class GuildNoticeUpdateHandler : InternalSessionMasterHandler<ProtoService.UpdateGuildNoticeRequest>
        {
            public GuildNoticeUpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateGuildNotice;

            protected override Task HandleMessage(ProtoService.UpdateGuildNoticeRequest message)
            {
                return _server.GuildManager.UpdateGuildNotice(message);
            }

            protected override ProtoService.UpdateGuildNoticeRequest Parse(ByteString content) => ProtoService.UpdateGuildNoticeRequest.Parser.ParseFrom(content);
        }

        internal class GuildRankChangeHandler : InternalSessionMasterHandler<ProtoService.UpdateGuildMemberRankRequest>
        {
            public GuildRankChangeHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.ChangeGuildMemberRank;

            protected override Task HandleMessage(ProtoService.UpdateGuildMemberRankRequest message)
            {
                return _server.GuildManager.ChangePlayerGuildRank(message);
            }

            protected override ProtoService.UpdateGuildMemberRankRequest Parse(ByteString content) => ProtoService.UpdateGuildMemberRankRequest.Parser.ParseFrom(content);
        }

        internal class GuildRankTitleUpdateHandler : InternalSessionMasterHandler<ProtoService.UpdateGuildRankTitleRequest>
        {
            public GuildRankTitleUpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateGuildRankTitle;

            protected override Task HandleMessage(ProtoService.UpdateGuildRankTitleRequest message)
            {
                return _server.GuildManager.UpdateGuildRankTitle(message);
            }

            protected override ProtoService.UpdateGuildRankTitleRequest Parse(ByteString content) => ProtoService.UpdateGuildRankTitleRequest.Parser.ParseFrom(content);
        }

        internal class GuildUpdateCapacity : InternalSessionMasterHandler<ProtoService.UpdateGuildCapacityRequest>
        {
            public GuildUpdateCapacity(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateGuildCapacity;

            protected override Task HandleMessage(ProtoService.UpdateGuildCapacityRequest message)
            {
                return _server.GuildManager.IncreseGuildCapacity(message);
            }

            protected override ProtoService.UpdateGuildCapacityRequest Parse(ByteString content) => ProtoService.UpdateGuildCapacityRequest.Parser.ParseFrom(content);
        }

        internal class GuildDropMessage : InternalSessionMasterHandler<ProtoService.GuildDropMessageRequest>
        {
            public GuildDropMessage(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.DropGuildMessage;

            protected override Task HandleMessage(ProtoService.GuildDropMessageRequest message)
            {
                return _server.GuildManager.SendGuildMessage(message.GuildId, message.Type, message.Message);
            }

            protected override ProtoService.GuildDropMessageRequest Parse(ByteString content) => ProtoService.GuildDropMessageRequest.Parser.ParseFrom(content);
        }
    }
}
