using Application.Shared.Internal;
using Application.Shared.Message;
using Google.Protobuf;
using GuildProto;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class GuildHandlers
    {
        public class InvokeGuildNoticeUpdateHandler : InternalSessionChannelHandler<GuildProto.UpdateGuildNoticeResponse>
        {
            public InvokeGuildNoticeUpdateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => ChannelRecvCode.OnGuildNoticeUpdate;

            protected override Task HandleAsync(UpdateGuildNoticeResponse res, CancellationToken cancellationToken = default)
            {
                _server.GuildManager.OnGuildNoticeUpdate(res);
                return Task.CompletedTask;
            }

            protected override UpdateGuildNoticeResponse Parse(ByteString data) => UpdateGuildNoticeResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildGpUpdateHandler : InternalSessionChannelHandler<GuildProto.UpdateGuildGPResponse>
        {
            public InvokeGuildGpUpdateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => ChannelRecvCode.OnGuildGpUpdate;

            protected override Task HandleAsync(UpdateGuildGPResponse res, CancellationToken cancellationToken = default)
            {
                _server.GuildManager.OnGuildGPUpdate(res);
                return Task.CompletedTask;
            }

            protected override UpdateGuildGPResponse Parse(ByteString data) => UpdateGuildGPResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildCapacityUpdateHandler : InternalSessionChannelHandler<GuildProto.UpdateGuildCapacityResponse>
        {
            public InvokeGuildCapacityUpdateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => ChannelRecvCode.OnGuildCapacityUpdate;

            protected override Task HandleAsync(UpdateGuildCapacityResponse res, CancellationToken cancellationToken = default)
            {
                _server.GuildManager.OnGuildCapacityIncreased(res);
                return Task.CompletedTask;
            }

            protected override UpdateGuildCapacityResponse Parse(ByteString data) => UpdateGuildCapacityResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildEmblemUpdateHandler : InternalSessionChannelHandler<GuildProto.UpdateGuildEmblemResponse>
        {
            public InvokeGuildEmblemUpdateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => ChannelRecvCode.OnGuildEmblemUpdate;

            protected override Task HandleAsync(UpdateGuildEmblemResponse res, CancellationToken cancellationToken = default)
            {
                _server.GuildManager.OnGuildEmblemUpdate(res);
                return Task.CompletedTask;
            }

            protected override UpdateGuildEmblemResponse Parse(ByteString data) => UpdateGuildEmblemResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildRankTitleUpdateHandler : InternalSessionChannelHandler<GuildProto.UpdateGuildRankTitleResponse>
        {
            public InvokeGuildRankTitleUpdateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => ChannelRecvCode.OnGuildRankTitleUpdate;

            protected override Task HandleAsync(UpdateGuildRankTitleResponse res, CancellationToken cancellationToken = default)
            {
                _server.GuildManager.OnGuildRankTitleUpdate(res);
                return Task.CompletedTask;
            }

            protected override UpdateGuildRankTitleResponse Parse(ByteString data) => UpdateGuildRankTitleResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildMemberRankUpdateHandler : InternalSessionChannelHandler<GuildProto.UpdateGuildMemberRankResponse>
        {
            public InvokeGuildMemberRankUpdateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => ChannelRecvCode.OnGuildRankChanged;

            protected override Task HandleAsync(UpdateGuildMemberRankResponse res, CancellationToken cancellationToken = default)
            {
                _server.GuildManager.OnChangePlayerGuildRank(res);
                return Task.CompletedTask;
            }

            protected override UpdateGuildMemberRankResponse Parse(ByteString data) => UpdateGuildMemberRankResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildMemberJoinHandler : InternalSessionChannelHandler<GuildProto.JoinGuildResponse>
        {
            public InvokeGuildMemberJoinHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => ChannelRecvCode.OnPlayerJoinGuild;

            protected override Task HandleAsync(JoinGuildResponse res, CancellationToken cancellationToken = default)
            {
                _server.GuildManager.OnPlayerJoinGuild(res);
                return Task.CompletedTask;
            }

            protected override JoinGuildResponse Parse(ByteString data) => JoinGuildResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildMemberLeaveHandler : InternalSessionChannelHandler<GuildProto.LeaveGuildResponse>
        {
            public InvokeGuildMemberLeaveHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => ChannelRecvCode.OnPlayerLeaveGuild;

            protected override Task HandleAsync(LeaveGuildResponse res, CancellationToken cancellationToken = default)
            {
                _server.GuildManager.OnPlayerLeaveGuild(res);
                return Task.CompletedTask;
            }

            protected override LeaveGuildResponse Parse(ByteString data) => LeaveGuildResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildExpelMemberHandler : InternalSessionChannelHandler<GuildProto.ExpelFromGuildResponse>
        {
            public InvokeGuildExpelMemberHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => ChannelRecvCode.OnGuildExpelMember;

            protected override Task HandleAsync(ExpelFromGuildResponse res, CancellationToken cancellationToken = default)
            {
                _server.GuildManager.OnGuildExpelMember(res);
                return Task.CompletedTask;
            }

            protected override ExpelFromGuildResponse Parse(ByteString data) => ExpelFromGuildResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildDisbandHandler : InternalSessionChannelHandler<GuildProto.GuildDisbandResponse>
        {
            public InvokeGuildDisbandHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => ChannelRecvCode.OnGuildDisband;

            protected override Task HandleAsync(GuildDisbandResponse res, CancellationToken cancellationToken = default)
            {
                _server.GuildManager.OnGuildDisband(res);
                return Task.CompletedTask;
            }

            protected override GuildDisbandResponse Parse(ByteString data) => GuildDisbandResponse.Parser.ParseFrom(data);
        }
    }
}
