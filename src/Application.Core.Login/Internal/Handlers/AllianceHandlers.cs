using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Login.Internal.Handlers
{
    internal class AllianceHandlers
    {
        internal class AllianceCapacityUpdateHandler : InternalSessionMasterHandler<ProtoService.IncreaseAllianceCapacityRequest>
        {
            public AllianceCapacityUpdateHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateAllianceCapacity;

            protected override Task HandleMessage(ProtoService.IncreaseAllianceCapacityRequest request)
            {
                return _server.GuildManager.IncreaseAllianceCapacity(request);
            }

            protected override ProtoService.IncreaseAllianceCapacityRequest Parse(ByteString data) => ProtoService.IncreaseAllianceCapacityRequest.Parser.ParseFrom(data);
        }

        internal class Disband : InternalSessionMasterHandler<ProtoService.DisbandAllianceRequest>
        {
            public Disband(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.DisbandAlliance;

            protected override Task HandleMessage(ProtoService.DisbandAllianceRequest request)
            {
                return _server.GuildManager.DisbandAlliance(request);
            }

            protected override ProtoService.DisbandAllianceRequest Parse(ByteString data) => ProtoService.DisbandAllianceRequest.Parser.ParseFrom(data);
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

        internal class GuildLeave : InternalSessionMasterHandler<ProtoService.GuildLeaveAllianceRequest>
        {
            public GuildLeave(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.LeaveAlliance;

            protected override Task HandleMessage(ProtoService.GuildLeaveAllianceRequest request)
            {
                return _server.GuildManager.GuildLeaveAlliance(request);
            }

            protected override ProtoService.GuildLeaveAllianceRequest Parse(ByteString data) => ProtoService.GuildLeaveAllianceRequest.Parser.ParseFrom(data);
        }

        internal class UpdateNotice : InternalSessionMasterHandler<ProtoService.UpdateAllianceNoticeRequest>
        {
            public UpdateNotice(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateAllianceNotice;

            protected override Task HandleMessage(ProtoService.UpdateAllianceNoticeRequest request)
            {
                return _server.GuildManager.UpdateAllianceNotice(request);
            }

            protected override ProtoService.UpdateAllianceNoticeRequest Parse(ByteString data) => ProtoService.UpdateAllianceNoticeRequest.Parser.ParseFrom(data);
        }


        internal class UpdateRankTitle : InternalSessionMasterHandler<ProtoService.UpdateAllianceRankTitleRequest>
        {
            public UpdateRankTitle(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateAllianceRankTitle;

            protected override Task HandleMessage(ProtoService.UpdateAllianceRankTitleRequest request)
            {
                return _server.GuildManager.UpdateAllianceRankTitle(request);
            }

            protected override ProtoService.UpdateAllianceRankTitleRequest Parse(ByteString data) => ProtoService.UpdateAllianceRankTitleRequest.Parser.ParseFrom(data);
        }


        internal class UpdateRank : InternalSessionMasterHandler<ProtoService.ChangePlayerAllianceRankRequest>
        {
            public UpdateRank(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateAllianceGuildRank;

            protected override Task HandleMessage(ProtoService.ChangePlayerAllianceRankRequest request)
            {
                return _server.GuildManager.ChangePlayerAllianceRank(request);
            }

            protected override ProtoService.ChangePlayerAllianceRankRequest Parse(ByteString data) => ProtoService.ChangePlayerAllianceRankRequest.Parser.ParseFrom(data);
        }

        internal class ChangeLeader : InternalSessionMasterHandler<ProtoService.AllianceChangeLeaderRequest>
        {
            public ChangeLeader(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.UpdateAllianceLeader;

            protected override Task HandleMessage(ProtoService.AllianceChangeLeaderRequest request)
            {
                return _server.GuildManager.ChangeAllianceLeader(request);
            }

            protected override ProtoService.AllianceChangeLeaderRequest Parse(ByteString data) => ProtoService.AllianceChangeLeaderRequest.Parser.ParseFrom(data);
        }

        internal class ExpelGuild : InternalSessionMasterHandler<ProtoService.AllianceExpelGuildRequest>
        {
            public ExpelGuild(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.ExpelAllianceGuild;

            protected override Task HandleMessage(ProtoService.AllianceExpelGuildRequest request)
            {
                return _server.GuildManager.AllianceExpelGuild(request);
            }

            protected override ProtoService.AllianceExpelGuildRequest Parse(ByteString data) => ProtoService.AllianceExpelGuildRequest.Parser.ParseFrom(data);
        }

    }
}
