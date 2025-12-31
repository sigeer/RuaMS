using AllianceProto;
using Application.Shared.Internal;
using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class AllianceHandlers
    {
        public class UpdateNotice : InternalSessionChannelHandler<UpdateAllianceNoticeResponse>
        {
            public UpdateNotice(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.OnAllianceNoticeUpdate;

            protected override Task HandleAsync(UpdateAllianceNoticeResponse res, CancellationToken cancellationToken = default)
            {
                _server.GuildManager.OnAllianceNoticeChanged(res);
                return Task.CompletedTask;
            }

            protected override UpdateAllianceNoticeResponse Parse(ByteString data) => UpdateAllianceNoticeResponse.Parser.ParseFrom(data);
        }

        public class JoinAlliance : InternalSessionChannelHandler<GuildJoinAllianceResponse>
        {
            public JoinAlliance(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.OnGuildJoinAlliance;

            protected override Task HandleAsync(GuildJoinAllianceResponse res, CancellationToken cancellationToken = default)
            {
                _server.GuildManager.OnGuildJoinAlliance(res);
                return Task.CompletedTask;
            }

            protected override GuildJoinAllianceResponse Parse(ByteString data) => GuildJoinAllianceResponse.Parser.ParseFrom(data);
        }

        public class LeaveAlliance : InternalSessionChannelHandler<GuildLeaveAllianceResponse>
        {
            public LeaveAlliance(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.OnGuildLeaveAlliance;

            protected override Task HandleAsync(GuildLeaveAllianceResponse res, CancellationToken cancellationToken = default)
            {
                _server.GuildManager.OnGuildLeaveAlliance(res);
                return Task.CompletedTask;
            }

            protected override GuildLeaveAllianceResponse Parse(ByteString data) => GuildLeaveAllianceResponse.Parser.ParseFrom(data);
        }

        public class ExpelGuild : InternalSessionChannelHandler<AllianceExpelGuildResponse>
        {
            public ExpelGuild(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.OnAllianceExpelGuild;

            protected override Task HandleAsync(AllianceExpelGuildResponse res, CancellationToken cancellationToken = default)
            {
                _server.GuildManager.OnAllianceExpelGuild(res);
                return Task.CompletedTask;
            }

            protected override AllianceExpelGuildResponse Parse(ByteString data) => AllianceExpelGuildResponse.Parser.ParseFrom(data);
        }

        public class IncreaseCapacity : InternalSessionChannelHandler<IncreaseAllianceCapacityResponse>
        {
            public IncreaseCapacity(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.OnAllianceCapacityUpdate;

            protected override Task HandleAsync(IncreaseAllianceCapacityResponse res, CancellationToken cancellationToken = default)
            {
                _server.GuildManager.OnAllianceCapacityIncreased(res);
                return Task.CompletedTask;
            }

            protected override IncreaseAllianceCapacityResponse Parse(ByteString data) => IncreaseAllianceCapacityResponse.Parser.ParseFrom(data);
        }

        public class DisbandAlliance : InternalSessionChannelHandler<DisbandAllianceResponse>
        {
            public DisbandAlliance(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.OnAllianceDisband;

            protected override Task HandleAsync(DisbandAllianceResponse res, CancellationToken cancellationToken = default)
            {
                _server.GuildManager.OnAllianceDisband(res);
                return Task.CompletedTask;
            }

            protected override DisbandAllianceResponse Parse(ByteString data) => DisbandAllianceResponse.Parser.ParseFrom(data);
        }

        public class UpdateAllianceRank : InternalSessionChannelHandler<ChangePlayerAllianceRankResponse>
        {
            public UpdateAllianceRank(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.OnAllianceMemberRankChanged;

            protected override Task HandleAsync(ChangePlayerAllianceRankResponse res, CancellationToken cancellationToken = default)
            {
                _server.GuildManager.OnPlayerAllianceRankChanged(res);
                return Task.CompletedTask;
            }

            protected override ChangePlayerAllianceRankResponse Parse(ByteString data) => ChangePlayerAllianceRankResponse.Parser.ParseFrom(data);
        }

        public class UpdateRankTitle : InternalSessionChannelHandler<UpdateAllianceRankTitleResponse>
        {
            public UpdateRankTitle(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.OnAllianceRankTitleUpdate;

            protected override Task HandleAsync(UpdateAllianceRankTitleResponse res, CancellationToken cancellationToken = default)
            {
                _server.GuildManager.OnAllianceRankTitleChanged(res);
                return Task.CompletedTask;
            }

            protected override UpdateAllianceRankTitleResponse Parse(ByteString data) => UpdateAllianceRankTitleResponse.Parser.ParseFrom(data);
        }

        public class ChangeLeader : InternalSessionChannelHandler<AllianceChangeLeaderResponse>
        {
            public ChangeLeader(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.OnAllianceLeaderChanged;

            protected override Task HandleAsync(AllianceChangeLeaderResponse res, CancellationToken cancellationToken = default)
            {
                _server.GuildManager.OnAllianceLeaderChanged(res);
                return Task.CompletedTask;
            }

            protected override AllianceChangeLeaderResponse Parse(ByteString data) => AllianceChangeLeaderResponse.Parser.ParseFrom(data);
        }
    }
}
