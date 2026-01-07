using AllianceProto;
using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Login.Internal.Handlers
{
    internal class AllianceHandlers
    {
        internal class AllianceCapacityUpdateHandler : InternalSessionMasterHandler<IncreaseAllianceCapacityRequest>
        {
            public AllianceCapacityUpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateAllianceCapacity;

            protected override async Task HandleAsync(IncreaseAllianceCapacityRequest request, CancellationToken cancellationToken = default)
            {
                await _server.GuildManager.IncreaseAllianceCapacity(request);
            }

            protected override IncreaseAllianceCapacityRequest Parse(ByteString data) => IncreaseAllianceCapacityRequest.Parser.ParseFrom(data);
        }

        internal class Disband : InternalSessionMasterHandler<DisbandAllianceRequest>
        {
            public Disband(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.DisbandAlliance;

            protected override async Task HandleAsync(DisbandAllianceRequest request, CancellationToken cancellationToken = default)
            {
                await _server.GuildManager.DisbandAlliance(request);
            }

            protected override DisbandAllianceRequest Parse(ByteString data) => DisbandAllianceRequest.Parser.ParseFrom(data);
        }

        //internal class Join : InternalSessionMasterHandler<GuildJoinAllianceRequest>
        //{
        //    public Join(MasterServer server) : base(server)
        //    {
        //    }

        //    public override int MessageId => (int)ChannelSendCode.JoinAlliance;

        //    protected override async Task HandleAsync(GuildJoinAllianceRequest request, CancellationToken cancellationToken = default)
        //    {
        //        await _server.GuildManager.GuildJoinAlliance(request);
        //    }

        //    protected override GuildJoinAllianceRequest Parse(ByteString data) => GuildJoinAllianceRequest.Parser.ParseFrom(data);
        //}

        internal class GuildLeave : InternalSessionMasterHandler<GuildLeaveAllianceRequest>
        {
            public GuildLeave(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.LeaveAlliance;

            protected override async Task HandleAsync(GuildLeaveAllianceRequest request, CancellationToken cancellationToken = default)
            {
                await _server.GuildManager.GuildLeaveAlliance(request);
            }

            protected override GuildLeaveAllianceRequest Parse(ByteString data) => GuildLeaveAllianceRequest.Parser.ParseFrom(data);
        }

        internal class UpdateNotice : InternalSessionMasterHandler<UpdateAllianceNoticeRequest>
        {
            public UpdateNotice(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateAllianceNotice;

            protected override async Task HandleAsync(UpdateAllianceNoticeRequest request, CancellationToken cancellationToken = default)
            {
                await _server.GuildManager.UpdateAllianceNotice(request);
            }

            protected override UpdateAllianceNoticeRequest Parse(ByteString data) => UpdateAllianceNoticeRequest.Parser.ParseFrom(data);
        }


        internal class UpdateRankTitle : InternalSessionMasterHandler<UpdateAllianceRankTitleRequest>
        {
            public UpdateRankTitle(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateAllianceRankTitle;

            protected override async Task HandleAsync(UpdateAllianceRankTitleRequest request, CancellationToken cancellationToken = default)
            {
                await _server.GuildManager.UpdateAllianceRankTitle(request);
            }

            protected override UpdateAllianceRankTitleRequest Parse(ByteString data) => UpdateAllianceRankTitleRequest.Parser.ParseFrom(data);
        }


        internal class UpdateRank : InternalSessionMasterHandler<AllianceProto.ChangePlayerAllianceRankRequest>
        {
            public UpdateRank(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateAllianceGuildRank;

            protected override async Task HandleAsync(ChangePlayerAllianceRankRequest request, CancellationToken cancellationToken = default)
            {
                await _server.GuildManager.ChangePlayerAllianceRank(request);
            }

            protected override ChangePlayerAllianceRankRequest Parse(ByteString data) => ChangePlayerAllianceRankRequest.Parser.ParseFrom(data);
        }

        internal class ChangeLeader : InternalSessionMasterHandler<AllianceProto.AllianceChangeLeaderRequest>
        {
            public ChangeLeader(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateAllianceLeader;

            protected override async Task HandleAsync(AllianceChangeLeaderRequest request, CancellationToken cancellationToken = default)
            {
                await _server.GuildManager.ChangeAllianceLeader(request);
            }

            protected override AllianceChangeLeaderRequest Parse(ByteString data) => AllianceChangeLeaderRequest.Parser.ParseFrom(data);
        }

        internal class ExpelGuild : InternalSessionMasterHandler<AllianceProto.AllianceExpelGuildRequest>
        {
            public ExpelGuild(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.ExpelAllianceGuild;

            protected override async Task HandleAsync(AllianceExpelGuildRequest request, CancellationToken cancellationToken = default)
            {
                await _server.GuildManager.AllianceExpelGuild(request);
            }

            protected override AllianceExpelGuildRequest Parse(ByteString data) => AllianceExpelGuildRequest.Parser.ParseFrom(data);
        }

    }
}
