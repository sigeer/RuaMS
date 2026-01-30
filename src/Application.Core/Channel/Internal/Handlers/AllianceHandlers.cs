using AllianceProto;
using Application.Core.Channel.Commands;
using Application.Core.Channel.Commands.Alliance;
using Application.Shared.Guild;
using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class AllianceHandlers
    {
        protected static string? GetErrorMessage(AllianceUpdateResult code)
        {
            switch (code)
            {
                case AllianceUpdateResult.Success:
                    break;
                case AllianceUpdateResult.PlayerNotExisted:
                    break;
                case AllianceUpdateResult.GuildNotExisted:
                    break;
                case AllianceUpdateResult.AllianceNotFound:
                    break;
                case AllianceUpdateResult.NotGuildLeader:
                    break;
                case AllianceUpdateResult.LeaderNotExisted:
                    break;
                case AllianceUpdateResult.AlreadyInAlliance:
                    break;
                case AllianceUpdateResult.NotAllianceLeader:
                    break;
                case AllianceUpdateResult.PlayerNotOnlined:
                    break;
                case AllianceUpdateResult.RankLimitted:
                    break;
                case AllianceUpdateResult.CapacityFull:
                    break;
                case AllianceUpdateResult.GuildNotInAlliance:
                    break;
                case AllianceUpdateResult.Create_NameInvalid:
                    break;
                case AllianceUpdateResult.Create_Error:
                    break;
                default:
                    return null;
            }
            return null;
        }
        public class CreateHandler : InternalSessionChannelHandler<AllianceProto.CreateAllianceResponse>
        {
            public CreateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnAllianceCreated;

            protected override void HandleMessage(CreateAllianceResponse res)
            {
                _server.PushChannelCommand(new CreateAllianceCallbackCommand(res));
                _server.GuildManager.StoreAlliance(res.Model);
            }

            protected override CreateAllianceResponse Parse(ByteString data) => CreateAllianceResponse.Parser.ParseFrom(data);
        }

        public class BroadcastPlayerInfo : InternalSessionChannelHandler<AllianceBroadcastPlayerInfoResponse>
        {
            public BroadcastPlayerInfo(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAlliancePlayerInfoBroadcast;

            protected override void HandleMessage(AllianceBroadcastPlayerInfoResponse res)
            {
                _server.PushChannelCommand(new BroadcastAllianceMemberInfoCommand(res));
            }

            protected override AllianceBroadcastPlayerInfoResponse Parse(ByteString data) => AllianceBroadcastPlayerInfoResponse.Parser.ParseFrom(data);
        }
        public class UpdateNotice : InternalSessionChannelHandler<UpdateAllianceNoticeResponse>
        {
            public UpdateNotice(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceNoticeUpdate;

            protected override void HandleMessage(UpdateAllianceNoticeResponse res)
            {
                _server.PushChannelCommand(new UpdateAllianceNoticeCallback(res));
                _server.GuildManager.ClearAllianceCache(res.AllianceId);
            }

            protected override UpdateAllianceNoticeResponse Parse(ByteString data) => UpdateAllianceNoticeResponse.Parser.ParseFrom(data);
        }

        public class JoinAlliance : InternalSessionChannelHandler<GuildJoinAllianceResponse>
        {
            public JoinAlliance(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnGuildJoinAlliance;

            protected override void HandleMessage(GuildJoinAllianceResponse res)
            {
                _server.PushChannelCommand(new JoinAllianceCallbackCommand(res));

                _server.GuildManager.StoreAlliance(res.AllianceDto);
            }

            protected override GuildJoinAllianceResponse Parse(ByteString data) => GuildJoinAllianceResponse.Parser.ParseFrom(data);
        }

        public class LeaveAlliance : InternalSessionChannelHandler<GuildLeaveAllianceResponse>
        {
            public LeaveAlliance(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnGuildLeaveAlliance;

            protected override void HandleMessage(GuildLeaveAllianceResponse res)
            {
                _server.PushChannelCommand(new LeaveAllianceCallbackCommands(res));

                _server.GuildManager.StoreAlliance(res.AllianceDto);
                _server.GuildManager.StoreGuild(res.GuildDto);

            }


            protected override GuildLeaveAllianceResponse Parse(ByteString data) => GuildLeaveAllianceResponse.Parser.ParseFrom(data);
        }

        public class ExpelGuild : InternalSessionChannelHandler<AllianceExpelGuildResponse>
        {
            public ExpelGuild(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceExpelGuild;

            protected override void HandleMessage(AllianceExpelGuildResponse res)
            {
                _server.PushChannelCommand(new ExpelGuildCallbackCommand(res));

                _server.GuildManager.StoreAlliance(res.AllianceDto);
                _server.GuildManager.StoreGuild(res.GuildDto);
            }

            protected override AllianceExpelGuildResponse Parse(ByteString data) => AllianceExpelGuildResponse.Parser.ParseFrom(data);
        }

        public class IncreaseCapacity : InternalSessionChannelHandler<IncreaseAllianceCapacityResponse>
        {
            public IncreaseCapacity(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceCapacityUpdate;

            protected override void HandleMessage(IncreaseAllianceCapacityResponse res)
            {
                _server.PushChannelCommand(new IncreaseAllianceCapacityCallbackCommand(res));
                _server.GuildManager.StoreAlliance(res.AllianceDto);
            }

            protected override IncreaseAllianceCapacityResponse Parse(ByteString data) => IncreaseAllianceCapacityResponse.Parser.ParseFrom(data);
        }

        public class DisbandAlliance : InternalSessionChannelHandler<DisbandAllianceResponse>
        {
            public DisbandAlliance(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceDisband;

            protected override void HandleMessage(DisbandAllianceResponse res)
            {
                _server.PushChannelCommand(new DisbandAllianceCallbackCommand(res));
                _server.GuildManager.ClearAllianceCache(res.AllianceId);
            }

            protected override DisbandAllianceResponse Parse(ByteString data) => DisbandAllianceResponse.Parser.ParseFrom(data);
        }

        public class UpdateAllianceRank : InternalSessionChannelHandler<ChangePlayerAllianceRankResponse>
        {
            public UpdateAllianceRank(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceMemberRankChanged;

            protected override void HandleMessage(ChangePlayerAllianceRankResponse res)
            {
                _server.PushChannelCommand(new UpdateMemberAllianceRankCallbackCommand(res));
                _server.GuildManager.StoreAlliance(res.AllianceDto);
            }

            protected override ChangePlayerAllianceRankResponse Parse(ByteString data) => ChangePlayerAllianceRankResponse.Parser.ParseFrom(data);
        }

        public class UpdateRankTitle : InternalSessionChannelHandler<UpdateAllianceRankTitleResponse>
        {
            public UpdateRankTitle(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceRankTitleUpdate;

            protected override void HandleMessage(UpdateAllianceRankTitleResponse res)
            {
                _server.PushChannelCommand(new UpdateAllianceRankTitleCallbackCommand(res));

                _server.GuildManager.ClearAllianceCache(res.AllianceId, false);
            }

            protected override UpdateAllianceRankTitleResponse Parse(ByteString data) => UpdateAllianceRankTitleResponse.Parser.ParseFrom(data);
        }

        public class ChangeLeader : InternalSessionChannelHandler<AllianceChangeLeaderResponse>
        {
            public ChangeLeader(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceLeaderChanged;

            protected override void HandleMessage(AllianceChangeLeaderResponse res)
            {
                _server.PushChannelCommand(new ChangeAllianceLeaderCallbackCommand(res));
                _server.GuildManager.StoreAlliance(res.AllianceDto);
            }

            protected override AllianceChangeLeaderResponse Parse(ByteString data) => AllianceChangeLeaderResponse.Parser.ParseFrom(data);
        }
    }
}
