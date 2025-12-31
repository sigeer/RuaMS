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

            public override int MessageId => ChannelSendCode.DisbandGuild;

            protected override async Task HandleAsync(GuildDisbandRequest message, CancellationToken cancellationToken = default)
            {
                await _server.GuildManager.DisbandGuild(message);
            }

            protected override GuildDisbandRequest Parse(ByteString content) => GuildDisbandRequest.Parser.ParseFrom(content);
        }
        internal class GuildMemberLeaveHandler : InternalSessionMasterHandler<GuildProto.LeaveGuildRequest>
        {
            public GuildMemberLeaveHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.LeaveGuild;

            protected override async Task HandleAsync(LeaveGuildRequest message, CancellationToken cancellationToken = default)
            {
                await _server.GuildManager.PlayerLeaveGuild(message);
            }

            protected override LeaveGuildRequest Parse(ByteString content) => LeaveGuildRequest.Parser.ParseFrom(content);
        }

        internal class GuildJoinHandler : InternalSessionMasterHandler<GuildProto.JoinGuildRequest>
        {
            public GuildJoinHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.JoinGuild;

            protected override async Task HandleAsync(JoinGuildRequest message, CancellationToken cancellationToken = default)
            {
                await _server.GuildManager.PlayerJoinGuild(message);
            }

            protected override JoinGuildRequest Parse(ByteString content) => JoinGuildRequest.Parser.ParseFrom(content);
        }

        internal class GuildExpelMemberHandler : InternalSessionMasterHandler<ExpelFromGuildRequest>
        {
            public GuildExpelMemberHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.ExpelGuildMember;

            protected override async Task HandleAsync(ExpelFromGuildRequest message, CancellationToken cancellationToken = default)
            {
                await _server.GuildManager.GuildExpelMember(message);
            }

            protected override ExpelFromGuildRequest Parse(ByteString content) => ExpelFromGuildRequest.Parser.ParseFrom(content);
        }

        internal class GuildGpUpdateHandler : InternalSessionMasterHandler<UpdateGuildGPRequest>
        {
            public GuildGpUpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.UpdateGuildGp;

            protected override async Task HandleAsync(UpdateGuildGPRequest message, CancellationToken cancellationToken = default)
            {
                await _server.GuildManager.UpdateGuildGPAsync(message);
            }

            protected override UpdateGuildGPRequest Parse(ByteString content) => UpdateGuildGPRequest.Parser.ParseFrom(content);
        }

        internal class GuildEmblemUpdateHandler : InternalSessionMasterHandler<GuildProto.UpdateGuildEmblemRequest>
        {
            public GuildEmblemUpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.UpdateGuildEmblem;

            protected override async Task HandleAsync(UpdateGuildEmblemRequest message, CancellationToken cancellationToken = default)
            {
                await _server.GuildManager.UpdateGuildEmblem(message);
            }

            protected override UpdateGuildEmblemRequest Parse(ByteString content) => UpdateGuildEmblemRequest.Parser.ParseFrom(content);
        }

        internal class GuildNoticeUpdateHandler : InternalSessionMasterHandler<UpdateGuildNoticeRequest>
        {
            public GuildNoticeUpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.UpdateGuildNotice;

            protected override async Task HandleAsync(UpdateGuildNoticeRequest message, CancellationToken cancellationToken = default)
            {
                await _server.GuildManager.UpdateGuildNotice(message);
            }

            protected override UpdateGuildNoticeRequest Parse(ByteString content) => UpdateGuildNoticeRequest.Parser.ParseFrom(content);
        }

        internal class GuildRankChangeHandler : InternalSessionMasterHandler<UpdateGuildMemberRankRequest>
        {
            public GuildRankChangeHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.ChangeGuildMemberRank;

            protected override async Task HandleAsync(UpdateGuildMemberRankRequest message, CancellationToken cancellationToken = default)
            {
                await _server.GuildManager.ChangePlayerGuildRank(message);
            }

            protected override UpdateGuildMemberRankRequest Parse(ByteString content) => UpdateGuildMemberRankRequest.Parser.ParseFrom(content);
        }

        internal class GuildRankTitleUpdateHandler : InternalSessionMasterHandler<UpdateGuildRankTitleRequest>
        {
            public GuildRankTitleUpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.UpdateGuildRankTitle;

            protected override async Task HandleAsync(UpdateGuildRankTitleRequest message, CancellationToken cancellationToken = default)
            {
                await _server.GuildManager.UpdateGuildRankTitle(message);
            }

            protected override UpdateGuildRankTitleRequest Parse(ByteString content) => UpdateGuildRankTitleRequest.Parser.ParseFrom(content);
        }

        internal class GuildUpdateCapacity : InternalSessionMasterHandler<UpdateGuildCapacityRequest>
        {
            public GuildUpdateCapacity(MasterServer server) : base(server)
            {
            }

            public override int MessageId => ChannelSendCode.UpdateGuildCapacity;

            protected override async Task HandleAsync(UpdateGuildCapacityRequest message, CancellationToken cancellationToken = default)
            {
                await _server.GuildManager.IncreseGuildCapacity(message);
            }

            protected override UpdateGuildCapacityRequest Parse(ByteString content) => UpdateGuildCapacityRequest.Parser.ParseFrom(content);
        }
    }
}
