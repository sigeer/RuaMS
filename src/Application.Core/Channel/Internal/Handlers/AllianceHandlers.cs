using AllianceProto;
using Application.Shared.Guild;
using Application.Shared.Message;
using Google.Protobuf;
using net.server.guild;

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

            protected override Task HandleAsync(CreateAllianceResponse res, CancellationToken cancellationToken = default)
            {
                if (res.Code != 0)
                {
                    var masterChr = _server.FindPlayerById(res.Request.Members[0]);
                    if (masterChr != null)
                    {
                        masterChr.GainMeso(res.Request.Cost, false);

                        masterChr.Client.NPCConversationManager?.sendOk("请检查一下你和另一个公会领袖是否都在这个房间里，确保两个公会目前都没有在联盟中注册。在这个过程中，除了你们两个，不应该有其他公会领袖在场。");
                        masterChr.Client.NPCConversationManager?.dispose();
                    }
                    return Task.CompletedTask;
                }

                foreach (var member in res.Model.Guilds.SelectMany(x => x.Members))
                {
                    var chr = _server.FindPlayerById(member.Id);
                    if (chr != null)
                    {
                        chr.SetAllianceSnapshot(res.Model);
                        chr.AllianceRank = member.AllianceRank;

                        chr.sendPacket(GuildPackets.UpdateAllianceInfo(res.Model));
                        // UpdateAllianceInfo有完整数据，这个包是否有必要？
                        chr.sendPacket(GuildPackets.allianceNotice(res.Model.AllianceId, res.Model.Notice));

                        if (chr.Id == res.Request.Members[0])
                        {
                            chr.Client.NPCConversationManager?.sendOk("已成功组建了家族联盟。");
                            chr.Client.NPCConversationManager?.dispose();
                        }
                    }
                }

                return Task.CompletedTask;
            }

            protected override CreateAllianceResponse Parse(ByteString data) => CreateAllianceResponse.Parser.ParseFrom(data);
        }

        public class BroadcastPlayerInfo : InternalSessionChannelHandler<AllianceBroadcastPlayerInfoResponse>
        {
            public BroadcastPlayerInfo(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAlliancePlayerInfoBroadcast;

            protected override Task HandleAsync(AllianceBroadcastPlayerInfoResponse res, CancellationToken cancellationToken = default)
            {
                if (res.Code != 0)
                {
                    return Task.CompletedTask;
                }

                foreach (var item in res.AllMembers)
                {
                    var chr = _server.FindPlayerById(item);
                    if (chr != null)
                    {
                        chr.sendPacket(GuildPackets.sendShowInfo(res.AllianceId, res.Request.MasterId));
                    }
                }
                return Task.CompletedTask;
            }

            protected override AllianceBroadcastPlayerInfoResponse Parse(ByteString data) => AllianceBroadcastPlayerInfoResponse.Parser.ParseFrom(data);
        }
        public class UpdateNotice : InternalSessionChannelHandler<UpdateAllianceNoticeResponse>
        {
            public UpdateNotice(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceNoticeUpdate;

            protected override Task HandleAsync(UpdateAllianceNoticeResponse res, CancellationToken cancellationToken = default)
            {
                if (res.Code != 0)
                {
                    return Task.CompletedTask;
                }

                foreach (var memberId in res.AllMembers)
                {
                    var chr = _server.FindPlayerById(memberId);
                    if (chr != null)
                    {
                        chr.sendPacket(GuildPackets.allianceNotice(res.AllianceId, res.Request.Notice));
                        chr.dropMessage(5, "* Alliance Notice : " + res.Request.Notice);
                    }
                }
                return Task.CompletedTask;
            }

            protected override UpdateAllianceNoticeResponse Parse(ByteString data) => UpdateAllianceNoticeResponse.Parser.ParseFrom(data);
        }

        public class JoinAlliance : InternalSessionChannelHandler<GuildJoinAllianceResponse>
        {
            public JoinAlliance(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnGuildJoinAlliance;

            protected override Task HandleAsync(GuildJoinAllianceResponse res, CancellationToken cancellationToken = default)
            {
                if (res.Code != 0)
                {
                    return Task.CompletedTask;
                }

                foreach (var member in res.AllianceDto.Guilds.SelectMany(x => x.Members))
                {
                    var chr = _server.FindPlayerById(member.Id);
                    if (chr != null)
                    {
                        // 似乎会被updateAllianceInfo覆盖
                        // chr.sendPacket(GuildPackets.addGuildToAlliance(res.AllianceDto, r));

                        chr.sendPacket(GuildPackets.UpdateAllianceInfo(res.AllianceDto));
                        // UpdateAllianceInfo有完整数据，这个包是否有必要？
                        chr.sendPacket(GuildPackets.allianceNotice(res.AllianceDto.AllianceId, res.AllianceDto.Notice));

                        if (chr.GuildId == res.GuildId)
                        {
                            chr.dropMessage("Your guild has joined the [" + res.AllianceDto.Name + "] union.");
                        }

                        chr.SetAllianceSnapshot(res.AllianceDto);
                        chr.AllianceRank = 5;
                        if (chr.GuildRank == 1)
                            chr.AllianceRank = 2;

                    }
                }

                _server.GuildManager.SetAlliance(res.AllianceDto);
                return Task.CompletedTask;
            }

            protected override GuildJoinAllianceResponse Parse(ByteString data) => GuildJoinAllianceResponse.Parser.ParseFrom(data);
        }

        public class LeaveAlliance : InternalSessionChannelHandler<GuildLeaveAllianceResponse>
        {
            public LeaveAlliance(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnGuildLeaveAlliance;

            protected override Task HandleAsync(GuildLeaveAllianceResponse res, CancellationToken cancellationToken = default)
            {
                if (res.Code != 0)
                {
                    return Task.CompletedTask;
                }

                foreach (var member in res.AllianceDto.Guilds.SelectMany(x => x.Members))
                {
                    var chr = _server.FindPlayerById(member.Id);
                    if (chr != null)
                    {
                        chr.sendPacket(GuildPackets.RemoveGuildFromAlliance(res.AllianceDto, res.GuildDto));

                        chr.sendPacket(GuildPackets.GetGuildAlliances(res.AllianceDto));
                        chr.sendPacket(GuildPackets.allianceNotice(res.AllianceId, res.AllianceDto.Notice));

                        chr.dropMessage("[" + res.GuildDto.Name + "] guild has left the union.");
                    }
                }

                foreach (var guildMember in res.GuildDto.Members)
                {
                    var chr = _server.FindPlayerById(guildMember.Id);
                    if (chr != null)
                    {
                        chr.sendPacket(GuildPackets.RemoveGuildFromAlliance(res.AllianceDto, res.GuildDto));

                        chr.sendPacket(GuildPackets.disbandAlliance(res.AllianceId));
                        chr.RemoveAllianceSnapshot();
                    }
                }

                _server.GuildManager.SetAlliance(res.AllianceDto);
                _server.GuildManager.SetGuild(res.GuildDto);
                return Task.CompletedTask;
            }


            protected override GuildLeaveAllianceResponse Parse(ByteString data) => GuildLeaveAllianceResponse.Parser.ParseFrom(data);
        }

        public class ExpelGuild : InternalSessionChannelHandler<AllianceExpelGuildResponse>
        {
            public ExpelGuild(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceExpelGuild;

            protected override Task HandleAsync(AllianceExpelGuildResponse res, CancellationToken cancellationToken = default)
            {
                if (res.Code != 0)
                {
                    return Task.CompletedTask;
                }

                foreach (var member in res.AllianceDto.Guilds.SelectMany(x => x.Members))
                {
                    var chr = _server.FindPlayerById(member.Id);
                    if (chr != null)
                    {
                        chr.sendPacket(GuildPackets.RemoveGuildFromAlliance(res.AllianceDto, res.GuildDto));

                        chr.sendPacket(GuildPackets.GetGuildAlliances(res.AllianceDto));
                        chr.sendPacket(GuildPackets.allianceNotice(res.AllianceId, res.AllianceDto.Notice));

                        chr.dropMessage("[" + res.GuildDto.Name + "] guild has been expelled from the union.");
                    }
                }

                foreach (var guildMember in res.GuildDto.Members)
                {
                    var chr = _server.FindPlayerById(guildMember.Id);
                    if (chr != null)
                    {
                        chr.sendPacket(GuildPackets.RemoveGuildFromAlliance(res.AllianceDto, res.GuildDto));

                        chr.sendPacket(GuildPackets.disbandAlliance(res.AllianceId));
                        chr.RemoveAllianceSnapshot();
                    }
                }

                _server.GuildManager.SetAlliance(res.AllianceDto);
                _server.GuildManager.SetGuild(res.GuildDto);
                return Task.CompletedTask;
            }

            protected override AllianceExpelGuildResponse Parse(ByteString data) => AllianceExpelGuildResponse.Parser.ParseFrom(data);
        }

        public class IncreaseCapacity : InternalSessionChannelHandler<IncreaseAllianceCapacityResponse>
        {
            public IncreaseCapacity(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceCapacityUpdate;

            protected override Task HandleAsync(IncreaseAllianceCapacityResponse res, CancellationToken cancellationToken = default)
            {
                if (res.Code != 0)
                {
                    return Task.CompletedTask;
                }

                foreach (var member in res.AllianceDto.Guilds.SelectMany(x => x.Members))
                {
                    var chr = _server.FindPlayerById(member.Id);
                    if (chr != null)
                    {
                        // 提升了容量，但是这两个数据包都与容量无关
                        //chr.sendPacket(GuildPackets.getGuildAlliances(alliance));
                        //chr.sendPacket(GuildPackets.allianceNotice(alliance.AllianceId, alliance.Notice));

                        if (chr.Id == res.Request.MasterId)
                        {
                            chr.sendPacket(GuildPackets.UpdateAllianceInfo(res.AllianceDto));
                        }
                        chr.SetAllianceSnapshot(res.AllianceDto);
                    }
                }

                _server.GuildManager.SetAlliance(res.AllianceDto);
                return Task.CompletedTask;
            }

            protected override IncreaseAllianceCapacityResponse Parse(ByteString data) => IncreaseAllianceCapacityResponse.Parser.ParseFrom(data);
        }

        public class DisbandAlliance : InternalSessionChannelHandler<DisbandAllianceResponse>
        {
            public DisbandAlliance(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceDisband;

            protected override Task HandleAsync(DisbandAllianceResponse res, CancellationToken cancellationToken = default)
            {
                if (res.Code != 0)
                {
                    return Task.CompletedTask;
                }

                foreach (var memberId in res.AllMembers)
                {
                    var chr = _server.FindPlayerById(memberId);
                    if (chr != null)
                    {
                        chr.sendPacket(GuildPackets.disbandAlliance(res.AllianceId));
                        chr.AllianceRank = 5;
                    }
                }

                _server.GuildManager.ClearAllianceCache(res.AllianceId);
                return Task.CompletedTask;
            }

            protected override DisbandAllianceResponse Parse(ByteString data) => DisbandAllianceResponse.Parser.ParseFrom(data);
        }

        public class UpdateAllianceRank : InternalSessionChannelHandler<ChangePlayerAllianceRankResponse>
        {
            public UpdateAllianceRank(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceMemberRankChanged;

            protected override Task HandleAsync(ChangePlayerAllianceRankResponse res, CancellationToken cancellationToken = default)
            {
                if (res.Code != 0)
                {
                    return Task.CompletedTask;
                }

                foreach (var member in res.AllianceDto.Guilds.SelectMany(x => x.Members))
                {
                    var chr = _server.FindPlayerById(member.Id);
                    if (chr != null)
                    {
                        chr.sendPacket(GuildPackets.GetGuildAlliances(res.AllianceDto));
                        chr.AllianceRank = res.NewRank;
                    }
                }
                _server.GuildManager.SetAlliance(res.AllianceDto);
                return Task.CompletedTask;
            }

            protected override ChangePlayerAllianceRankResponse Parse(ByteString data) => ChangePlayerAllianceRankResponse.Parser.ParseFrom(data);
        }

        public class UpdateRankTitle : InternalSessionChannelHandler<UpdateAllianceRankTitleResponse>
        {
            public UpdateRankTitle(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceRankTitleUpdate;

            protected override Task HandleAsync(UpdateAllianceRankTitleResponse res, CancellationToken cancellationToken = default)
            {
                if (res.Code != 0)
                {
                    return Task.CompletedTask;
                }

                foreach (var memberId in res.AllMembers)
                {
                    var chr = _server.FindPlayerById(memberId);
                    if (chr != null)
                    {
                        chr.sendPacket(GuildPackets.changeAllianceRankTitle(res.AllianceId, res.Request.RankTitles.ToArray()));
                    }
                }
                _server.GuildManager.ClearAllianceCache(res.AllianceId, false);
                return Task.CompletedTask;
            }

            protected override UpdateAllianceRankTitleResponse Parse(ByteString data) => UpdateAllianceRankTitleResponse.Parser.ParseFrom(data);
        }

        public class ChangeLeader : InternalSessionChannelHandler<AllianceChangeLeaderResponse>
        {
            public ChangeLeader(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceLeaderChanged;

            protected override Task HandleAsync(AllianceChangeLeaderResponse res, CancellationToken cancellationToken = default)
            {
                if (res.Code != 0)
                {
                    return Task.CompletedTask;
                }


                foreach (var member in res.AllianceDto.Guilds.SelectMany(x => x.Members))
                {
                    var chr = _server.FindPlayerById(member.Id);
                    if (chr != null)
                    {
                        if (chr.Id == res.Request.MasterId)
                        {
                            chr.AllianceRank = 2;
                        }
                        else if (chr.Id == res.Request.PlayerId)
                        {
                            chr.AllianceRank = 1;
                        }

                        chr.sendPacket(GuildPackets.GetGuildAlliances(res.AllianceDto));
                        chr.dropMessage("'" + res.NewLeaderName + "' has been appointed as the new head of this Alliance.");
                    }
                }
                _server.GuildManager.SetAlliance(res.AllianceDto);
                return Task.CompletedTask;
            }

            protected override AllianceChangeLeaderResponse Parse(ByteString data) => AllianceChangeLeaderResponse.Parser.ParseFrom(data);
        }
    }
}
