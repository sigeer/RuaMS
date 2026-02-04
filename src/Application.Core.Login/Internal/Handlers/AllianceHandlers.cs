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

            protected override void HandleMessage(IncreaseAllianceCapacityRequest request)
            {
                _ = _server.GuildManager.IncreaseAllianceCapacity(request);
            }

            protected override IncreaseAllianceCapacityRequest Parse(ByteString data) => IncreaseAllianceCapacityRequest.Parser.ParseFrom(data);
        }

        internal class Disband : InternalSessionMasterHandler<DisbandAllianceRequest>
        {
            public Disband(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.DisbandAlliance;

            protected override void HandleMessage(DisbandAllianceRequest request)
            {
                _ = _server.GuildManager.DisbandAlliance(request);
            }

            protected override DisbandAllianceRequest Parse(ByteString data) => DisbandAllianceRequest.Parser.ParseFrom(data);
        }

        //internal class Join : InternalSessionMasterHandler<GuildJoinAllianceRequest>
        //{
        //    public Join(MasterServer server) : base(server)
        //    {
        //    }

        //    public override int MessageId => (int)ChannelSendCode.JoinAlliance;

        //    protected override void HandleAsync(GuildJoinAllianceRequest request, CancellationToken cancellationToken = default)
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

            protected override void HandleMessage(GuildLeaveAllianceRequest request)
            {
                _ = _server.GuildManager.GuildLeaveAlliance(request);
            }

            protected override GuildLeaveAllianceRequest Parse(ByteString data) => GuildLeaveAllianceRequest.Parser.ParseFrom(data);
        }

        internal class UpdateNotice : InternalSessionMasterHandler<UpdateAllianceNoticeRequest>
        {
            public UpdateNotice(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateAllianceNotice;

            protected override void HandleMessage(UpdateAllianceNoticeRequest request)
            {
                _ = _server.GuildManager.UpdateAllianceNotice(request);
            }

            protected override UpdateAllianceNoticeRequest Parse(ByteString data) => UpdateAllianceNoticeRequest.Parser.ParseFrom(data);
        }


        internal class UpdateRankTitle : InternalSessionMasterHandler<UpdateAllianceRankTitleRequest>
        {
            public UpdateRankTitle(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateAllianceRankTitle;

            protected override void HandleMessage(UpdateAllianceRankTitleRequest request)
            {
                _ = _server.GuildManager.UpdateAllianceRankTitle(request);
            }

            protected override UpdateAllianceRankTitleRequest Parse(ByteString data) => UpdateAllianceRankTitleRequest.Parser.ParseFrom(data);
        }


        internal class UpdateRank : InternalSessionMasterHandler<AllianceProto.ChangePlayerAllianceRankRequest>
        {
            public UpdateRank(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateAllianceGuildRank;

            protected override void HandleMessage(ChangePlayerAllianceRankRequest request)
            {
                _ = _server.GuildManager.ChangePlayerAllianceRank(request);
            }

            protected override ChangePlayerAllianceRankRequest Parse(ByteString data) => ChangePlayerAllianceRankRequest.Parser.ParseFrom(data);
        }

        internal class ChangeLeader : InternalSessionMasterHandler<AllianceProto.AllianceChangeLeaderRequest>
        {
            public ChangeLeader(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateAllianceLeader;

            protected override void HandleMessage(AllianceChangeLeaderRequest request)
            {
                _ = _server.GuildManager.ChangeAllianceLeader(request);
            }

            protected override AllianceChangeLeaderRequest Parse(ByteString data) => AllianceChangeLeaderRequest.Parser.ParseFrom(data);
        }

        internal class ExpelGuild : InternalSessionMasterHandler<AllianceProto.AllianceExpelGuildRequest>
        {
            public ExpelGuild(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.ExpelAllianceGuild;

            protected override void HandleMessage(AllianceExpelGuildRequest request)
            {
                _ = _server.GuildManager.AllianceExpelGuild(request);
            }

            protected override AllianceExpelGuildRequest Parse(ByteString data) => AllianceExpelGuildRequest.Parser.ParseFrom(data);
        }

    }
}
