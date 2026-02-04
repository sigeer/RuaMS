using Application.Core.Channel.Commands;
using Application.Shared.Message;
using Application.Shared.Team;
using Google.Protobuf;
using GuildProto;
using net.server.guild;
using tools;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class GuildHandlers
    {
        public static string? GetErrorMessage(GuildUpdateResult code)
        {
            switch (code)
            {
                case GuildUpdateResult.Success:
                    break;
                case GuildUpdateResult.PlayerNotExisted:
                    break;
                case GuildUpdateResult.PlayerNotOnlined:
                    break;
                case GuildUpdateResult.GuildNotExisted:
                    return "家族已经解散";
                case GuildUpdateResult.GuildFull:
                    return "家族人员已满";
                case GuildUpdateResult.MasterRankFail:
                    return "权限不足";
                case GuildUpdateResult.Join_AlreadyInGuild:
                    return "已经在家族里了";
                case GuildUpdateResult.LeaderCannotLeave:
                    return "族长不能离开";
                case GuildUpdateResult.Create_AlreadyInGuild:
                    return "You cannot create a new Guild while in one.";
                case GuildUpdateResult.Create_MemberAlreadyInGuild:
                    return "Please make sure everyone you are trying to invite is neither on a guild.";
                case GuildUpdateResult.Create_LeaderRequired:
                    return "You cannot establish the creation of a new Guild without leading a party.";
                case GuildUpdateResult.Create_MapRequired:
                    return "You cannot establish the creation of a new Guild outside of the Guild Headquarters.";
                case GuildUpdateResult.Create_MemberNotHere:
                    return "You cannot establish the creation of a new Guild if one of the members is not present here.";
                case GuildUpdateResult.Create_NameDumplicate:
                    return "The Guild name you have chosen is not accepted.";
                default:
                    break;
            }
            return null;
        }

        public class CreateHandler : InternalSessionChannelHandler<GuildProto.CreateGuildResponse>
        {
            public CreateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnGuildCreated;

            protected override void HandleMessage(CreateGuildResponse res)
            {
                _server.PushChannelCommand(new InvokeGuildCreateCallbackCommand(res));
                _server.GuildManager.StoreGuild(res.GuildDto);
            }

            protected override CreateGuildResponse Parse(ByteString data) => CreateGuildResponse.Parser.ParseFrom(data);
        }

        public class GuildMemberServerChangedHandler : InternalSessionChannelHandler<GuildProto.GuildMemberServerChangedResponse>
        {
            public GuildMemberServerChangedHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnGuildMemberLoginOff;

            protected override void HandleMessage(GuildMemberServerChangedResponse res)
            {
                _server.PushChannelCommand(new InvokeGuildMemberServerChangeCallbackCommand(res));

                if (res.AllianceId > 0)
                {
                    _server.GuildManager.ClearAllianceCache(res.AllianceId);
                }
                else
                {
                    _server.GuildManager.ClearGuildCache(res.GuildId);
                }
            }

            protected override GuildMemberServerChangedResponse Parse(ByteString data) => GuildMemberServerChangedResponse.Parser.ParseFrom(data);
        }


        public class GuildMemberUpdateHandler : InternalSessionChannelHandler<GuildProto.GuildMemberUpdateResponse>
        {
            public GuildMemberUpdateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnGuildMemberUpdate;

            protected override void HandleMessage(GuildMemberUpdateResponse res)
            {
                _server.PushChannelCommand(new InvokeGuildMemberUpdateCallbackCommand(res));
                if (res.AllianceId > 0)
                {
                    _server.GuildManager.ClearAllianceCache(res.AllianceId);
                }
                else
                {
                    _server.GuildManager.ClearGuildCache(res.GuildId);
                }

            }

            protected override GuildMemberUpdateResponse Parse(ByteString data) => GuildMemberUpdateResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildNoticeUpdateHandler : InternalSessionChannelHandler<GuildProto.UpdateGuildNoticeResponse>
        {
            public InvokeGuildNoticeUpdateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnGuildNoticeUpdate;

            protected override void HandleMessage(UpdateGuildNoticeResponse res)
            {
                var resCode = (GuildUpdateResult)res.Code;
                if (resCode != GuildUpdateResult.Success)
                {
                    return;
                }

                _server.PushChannelCommand(new InvokeGuildNoticeUpdateCallbackCommand(res));

                _server.GuildManager.ClearGuildCache(res.GuildId);
            }

            protected override UpdateGuildNoticeResponse Parse(ByteString data) => UpdateGuildNoticeResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildGpUpdateHandler : InternalSessionChannelHandler<GuildProto.UpdateGuildGPResponse>
        {
            public InvokeGuildGpUpdateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnGuildGpUpdate;

            protected override void HandleMessage(UpdateGuildGPResponse res)
            {
                var resCode = (GuildUpdateResult)res.Code;
                if (resCode != GuildUpdateResult.Success)
                {
                    return;
                }


                _server.PushChannelCommand(new InvokeGuildGpUpdateCommand(res));
                _server.GuildManager.ClearGuildCache(res.GuildId);
            }

            protected override UpdateGuildGPResponse Parse(ByteString data) => UpdateGuildGPResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildCapacityUpdateHandler : InternalSessionChannelHandler<GuildProto.UpdateGuildCapacityResponse>
        {
            public InvokeGuildCapacityUpdateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnGuildCapacityUpdate;

            protected override void HandleMessage(UpdateGuildCapacityResponse res)
            {
                _server.PushChannelCommand(new InvokeGuildGpUpdateCallbackCommand(res));

                _server.GuildManager.ClearGuildCache(res.GuildId);
            }

            protected override UpdateGuildCapacityResponse Parse(ByteString data) => UpdateGuildCapacityResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildEmblemUpdateHandler : InternalSessionChannelHandler<GuildProto.UpdateGuildEmblemResponse>
        {
            public InvokeGuildEmblemUpdateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnGuildEmblemUpdate;

            protected override void HandleMessage(UpdateGuildEmblemResponse res)
            {
                _server.PushChannelCommand(new InvokeGuildEmblemUpdateCallbackCommand(res));

                _server.GuildManager.ClearGuildCache(res.GuildId);
                _server.GuildManager.StoreAlliance(res.AllianceDto);
            }

            protected override UpdateGuildEmblemResponse Parse(ByteString data) => UpdateGuildEmblemResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildRankTitleUpdateHandler : InternalSessionChannelHandler<GuildProto.UpdateGuildRankTitleResponse>
        {
            public InvokeGuildRankTitleUpdateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnGuildRankTitleUpdate;

            protected override void HandleMessage(UpdateGuildRankTitleResponse res)
            {
                var resCode = (GuildUpdateResult)res.Code;
                if (resCode != GuildUpdateResult.Success)
                {
                    return;
                }

                _server.PushChannelCommand(new InvokeGuildRankTitleUpdateCallbackCommand(res));

                _server.GuildManager.ClearGuildCache(res.GuildId);
            }

            protected override UpdateGuildRankTitleResponse Parse(ByteString data) => UpdateGuildRankTitleResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildMemberRankUpdateHandler : InternalSessionChannelHandler<GuildProto.UpdateGuildMemberRankResponse>
        {
            public InvokeGuildMemberRankUpdateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnGuildRankChanged;

            protected override void HandleMessage(UpdateGuildMemberRankResponse res)
            {
                var resCode = (GuildUpdateResult)res.Code;
                if (resCode != GuildUpdateResult.Success)
                {
                    return;
                }

                _server.PushChannelCommand(new InvokeGuildMemberRankUpdateCallbackCommand(res));

                _server.GuildManager.ClearGuildCache(res.GuildId);
            }

            protected override UpdateGuildMemberRankResponse Parse(ByteString data) => UpdateGuildMemberRankResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildMemberJoinHandler : InternalSessionChannelHandler<GuildProto.JoinGuildResponse>
        {
            public InvokeGuildMemberJoinHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnPlayerJoinGuild;

            protected override void HandleMessage(JoinGuildResponse res)
            {
                _server.PushChannelCommand(new InvokeGuildMemberJoinCallbackCommand(res));

                _server.GuildManager.StoreGuild(res.GuildDto);
                _server.GuildManager.StoreAlliance(res.AllianceDto);
            }

            protected override JoinGuildResponse Parse(ByteString data) => JoinGuildResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildMemberLeaveHandler : InternalSessionChannelHandler<GuildProto.LeaveGuildResponse>
        {
            public InvokeGuildMemberLeaveHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnPlayerLeaveGuild;

            protected override void HandleMessage(LeaveGuildResponse res)
            {
                _server.PushChannelCommand(new InvokeGuildMemberLeaveCallbackCommand(res));

                _server.GuildManager.ClearGuildCache(res.GuildId);
                _server.GuildManager.StoreAlliance(res.AllianceDto);
            }

            protected override LeaveGuildResponse Parse(ByteString data) => LeaveGuildResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildExpelMemberHandler : InternalSessionChannelHandler<GuildProto.ExpelFromGuildResponse>
        {
            public InvokeGuildExpelMemberHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnGuildExpelMember;

            protected override void HandleMessage(ExpelFromGuildResponse res)
            {
                _server.PushChannelCommand(new InvokeGuildExpelMemberCallbackCommand(res));

                _server.GuildManager.ClearGuildCache(res.GuildId);
                _server.GuildManager.StoreAlliance(res.AllianceDto);
            }

            protected override ExpelFromGuildResponse Parse(ByteString data) => ExpelFromGuildResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildDisbandHandler : InternalSessionChannelHandler<GuildProto.GuildDisbandResponse>
        {
            public InvokeGuildDisbandHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnGuildDisband;

            protected override void HandleMessage(GuildDisbandResponse res)
            {
                _server.PushChannelCommand(new InvokeGuildDisbandCallbackCommand(res));

                _server.GuildManager.ClearGuildCache(res.GuildId);
                _server.GuildManager.StoreAlliance(res.AllianceDto);
            }

            protected override GuildDisbandResponse Parse(ByteString data) => GuildDisbandResponse.Parser.ParseFrom(data);
        }
    }
}
