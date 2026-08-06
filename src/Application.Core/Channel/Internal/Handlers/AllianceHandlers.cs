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
        public class CreateHandler : InternalSessionChannelHandler<ProtoService.CreateAllianceResponse>
        {
            public CreateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnAllianceCreated;

            protected override Task HandleMessage(ProtoService.CreateAllianceResponse res)
            {
                _server.Broadcast(w =>
                {
                    if (res.Code != 0)
                    {
                        w.getPlayerStorage().GetCharacterActor(res.Request.Members[0])?.Send(async m =>
                            {
                                var masterChr = m.getCharacterById(res.Request.Members[0]);
                                if (masterChr != null)
                                {
                                    await masterChr.GainMeso(res.Request.Cost);
                                    await masterChr.Dialog("请检查一下你和另一个公会领袖是否都在这个房间里，确保两个公会目前都没有在联盟中注册。在这个过程中，除了你们两个，不应该有其他公会领袖在场。");
                                }
                            });
                        return;
                    }

                    foreach (var member in res.Model.Guilds.SelectMany(x => x.Members))
                    {
                        w.getPlayerStorage().GetCharacterActor(member.Id)?.Send(async m =>
                            {
                                var chr = m.getCharacterById(member.Id);
                                if (chr != null)
                                {
                                    chr.AllianceRank = member.AllianceRank;
                                    await chr.SendPacket(GuildPackets.UpdateAllianceInfo(res.Model));
                                    await chr.SendPacket(GuildPackets.allianceNotice(res.Model.AllianceId, res.Model.Notice));
                                    if (chr.Id == res.Request.Members[0])
                                    {
                                        await chr.Dialog("已成功组建了家族联盟。");
                                    }
                                }
                            });
                    }
                });
                _server.GuildManager.StoreAlliance(res.Model);
                return Task.CompletedTask;
            }

            protected override ProtoService.CreateAllianceResponse Parse(ByteString data) => ProtoService.CreateAllianceResponse.Parser.ParseFrom(data);
        }

        public class BroadcastPlayerInfo : InternalSessionChannelHandler<ProtoService.AllianceBroadcastPlayerInfoResponse>
        {
            public BroadcastPlayerInfo(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAlliancePlayerInfoBroadcast;

            protected override async Task HandleMessage(ProtoService.AllianceBroadcastPlayerInfoResponse res)
            {
                if (res.Code != 0)
                {
                    return;
                }

                await _server.SendToPlayersAsync(res.AllMembers, async chr =>
                {
                    await chr.SendPacket(GuildPackets.sendShowInfo(res.AllianceId, res.Request.MasterId));
                });
            }

            protected override ProtoService.AllianceBroadcastPlayerInfoResponse Parse(ByteString data) => ProtoService.AllianceBroadcastPlayerInfoResponse.Parser.ParseFrom(data);
        }
        public class UpdateNotice : InternalSessionChannelHandler<ProtoService.UpdateAllianceNoticeResponse>
        {
            public UpdateNotice(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceNoticeUpdate;

            protected override async Task HandleMessage(ProtoService.UpdateAllianceNoticeResponse res)
            {
                if (res.Code != 0)
                {
                    return;
                }

                _server.GuildManager.ClearAllianceCache(res.AllianceId);

                await _server.SendToPlayersAsync(res.AllMembers, async chr =>
                {
                    await chr.SendPacket(GuildPackets.allianceNotice(res.AllianceId, res.Request.Notice));
                    await chr.dropMessage(5, "* Alliance Notice : " + res.Request.Notice);
                });
            }

            protected override ProtoService.UpdateAllianceNoticeResponse Parse(ByteString data) => ProtoService.UpdateAllianceNoticeResponse.Parser.ParseFrom(data);
        }

        public class JoinAlliance : InternalSessionChannelHandler<ProtoService.GuildJoinAllianceResponse>
        {
            public JoinAlliance(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnGuildJoinAlliance;

            protected override async Task HandleMessage(ProtoService.GuildJoinAllianceResponse res)
            {
                if (res.Code != 0)
                {
                    return;
                }

                _server.GuildManager.StoreAlliance(res.AllianceDto);
                var allMembers = res.AllianceDto.Guilds.SelectMany(x => x.Members).Select(x => x.Id);

                await _server.SendToPlayersAsync(allMembers, async chr =>
                {
                    await chr.SendPacket(GuildPackets.UpdateAllianceInfo(res.AllianceDto));
                    await chr.SendPacket(GuildPackets.allianceNotice(res.AllianceDto.AllianceId, res.AllianceDto.Notice));

                    if (chr.GuildId == res.GuildId)
                    {
                        await chr.Notice("Your guild has joined the [" + res.AllianceDto.Name + "] union.");
                    }

                    chr.AllianceRank = 5;
                    if (chr.GuildRank == 1)
                        chr.AllianceRank = 2;
                });
            }

            protected override ProtoService.GuildJoinAllianceResponse Parse(ByteString data) => ProtoService.GuildJoinAllianceResponse.Parser.ParseFrom(data);
        }

        public class LeaveAlliance : InternalSessionChannelHandler<ProtoService.GuildLeaveAllianceResponse>
        {
            public LeaveAlliance(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnGuildLeaveAlliance;

            protected override async Task HandleMessage(ProtoService.GuildLeaveAllianceResponse res)
            {
                if (res.Code != 0)
                {
                    return;
                }

                _server.GuildManager.StoreAlliance(res.AllianceDto);
                _server.GuildManager.StoreGuild(res.GuildDto);

                var allMembers = res.AllianceDto.Guilds.SelectMany(x => x.Members).Select(x => x.Id).ToHashSet();
                var guildMembers = res.GuildDto.Members.Select(x => x.Id).ToHashSet();
                await _server.SendToPlayersAsync([.. allMembers, .. guildMembers], async chr =>
                {
                    if (allMembers.Contains(chr.Id))
                    {
                        await chr.SendPacket(GuildPackets.RemoveGuildFromAlliance(res.AllianceDto, res.GuildDto));
                        await chr.SendPacket(GuildPackets.GetGuildAlliances(res.AllianceDto));
                        await chr.SendPacket(GuildPackets.allianceNotice(res.AllianceId, res.AllianceDto.Notice));
                        await chr.Notice("[" + res.GuildDto.Name + "] guild has left the union.");
                    }
                    else if (guildMembers.Contains(chr.Id))
                    {
                        await chr.SendPacket(GuildPackets.RemoveGuildFromAlliance(res.AllianceDto, res.GuildDto));
                        await chr.SendPacket(GuildPackets.disbandAlliance(res.AllianceId));
                    }
                });
            }


            protected override ProtoService.GuildLeaveAllianceResponse Parse(ByteString data) => ProtoService.GuildLeaveAllianceResponse.Parser.ParseFrom(data);
        }

        public class ExpelGuild : InternalSessionChannelHandler<ProtoService.AllianceExpelGuildResponse>
        {
            public ExpelGuild(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceExpelGuild;

            protected override async Task HandleMessage(ProtoService.AllianceExpelGuildResponse res)
            {
                if (res.Code != 0)
                {
                    return;
                }

                _server.GuildManager.StoreAlliance(res.AllianceDto);
                _server.GuildManager.StoreGuild(res.GuildDto);

                var allMembers = res.AllianceDto.Guilds.SelectMany(x => x.Members).Select(x => x.Id).ToHashSet();
                var guildMembers = res.GuildDto.Members.Select(x => x.Id).ToHashSet();
                await _server.SendToPlayersAsync([.. allMembers, .. guildMembers], async chr =>
                {
                    if (allMembers.Contains(chr.Id))
                    {
                        await chr.SendPacket(GuildPackets.RemoveGuildFromAlliance(res.AllianceDto, res.GuildDto));
                        await chr.SendPacket(GuildPackets.GetGuildAlliances(res.AllianceDto));
                        await chr.SendPacket(GuildPackets.allianceNotice(res.AllianceId, res.AllianceDto.Notice));
                        await chr.Notice("[" + res.GuildDto.Name + "] guild has been expelled from the union.");
                    }
                    else if (guildMembers.Contains(chr.Id))
                    {
                        await chr.SendPacket(GuildPackets.RemoveGuildFromAlliance(res.AllianceDto, res.GuildDto));
                        await chr.SendPacket(GuildPackets.disbandAlliance(res.AllianceId));
                    }
                });
            }

            protected override ProtoService.AllianceExpelGuildResponse Parse(ByteString data) => ProtoService.AllianceExpelGuildResponse.Parser.ParseFrom(data);
        }

        public class IncreaseCapacity : InternalSessionChannelHandler<ProtoService.IncreaseAllianceCapacityResponse>
        {
            public IncreaseCapacity(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceCapacityUpdate;

            protected override async Task HandleMessage(ProtoService.IncreaseAllianceCapacityResponse res)
            {
                if (res.Code != 0)
                {
                    return;
                }

                _server.GuildManager.StoreAlliance(res.AllianceDto);

                var allMembers = res.AllianceDto.Guilds.SelectMany(x => x.Members).Select(x => x.Id).ToHashSet();
                await _server.SendToPlayersAsync(allMembers, async chr =>
                {
                    await chr.SendPacket(GuildPackets.UpdateAllianceInfo(res.AllianceDto));
                });
            }

            protected override ProtoService.IncreaseAllianceCapacityResponse Parse(ByteString data) => ProtoService.IncreaseAllianceCapacityResponse.Parser.ParseFrom(data);
        }

        public class DisbandAlliance : InternalSessionChannelHandler<ProtoService.DisbandAllianceResponse>
        {
            public DisbandAlliance(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceDisband;

            protected override async Task HandleMessage(ProtoService.DisbandAllianceResponse res)
            {
                if (res.Code != 0)
                {
                    return;
                }
                _server.GuildManager.ClearAllianceCache(res.AllianceId);

                await _server.SendToPlayersAsync(res.AllMembers, async chr =>
                {
                    await chr.SendPacket(GuildPackets.disbandAlliance(res.AllianceId));
                    chr.AllianceRank = 5;
                });
            }

            protected override ProtoService.DisbandAllianceResponse Parse(ByteString data) => ProtoService.DisbandAllianceResponse.Parser.ParseFrom(data);
        }

        public class UpdateAllianceRank : InternalSessionChannelHandler<ProtoService.ChangePlayerAllianceRankResponse>
        {
            public UpdateAllianceRank(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceMemberRankChanged;

            protected override async Task HandleMessage(ProtoService.ChangePlayerAllianceRankResponse res)
            {
                if (res.Code != 0)
                {
                    return;
                }

                _server.GuildManager.StoreAlliance(res.AllianceDto);

                var allMembers = res.AllianceDto.Guilds.SelectMany(x => x.Members).Select(x => x.Id);
                await _server.SendToPlayersAsync(allMembers, async chr =>
                {
                    await chr.SendPacket(GuildPackets.GetGuildAlliances(res.AllianceDto));

                    if (chr.Id == res.Request.PlayerId)
                    {
                        chr.AllianceRank = res.NewRank;
                    }
                });
            }

            protected override ProtoService.ChangePlayerAllianceRankResponse Parse(ByteString data) => ProtoService.ChangePlayerAllianceRankResponse.Parser.ParseFrom(data);
        }

        public class UpdateRankTitle : InternalSessionChannelHandler<ProtoService.UpdateAllianceRankTitleResponse>
        {
            public UpdateRankTitle(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceRankTitleUpdate;

            protected override async Task HandleMessage(ProtoService.UpdateAllianceRankTitleResponse res)
            {
                if (res.Code != 0)
                {
                    return;
                }

                _server.GuildManager.ClearAllianceCache(res.AllianceId, false);
                await _server.SendToPlayersAsync(res.AllMembers, async chr =>
                {
                    await chr.SendPacket(GuildPackets.changeAllianceRankTitle(res.AllianceId, res.Request.RankTitles.ToArray()));
                });
            }

            protected override ProtoService.UpdateAllianceRankTitleResponse Parse(ByteString data) => ProtoService.UpdateAllianceRankTitleResponse.Parser.ParseFrom(data);
        }

        public class ChangeLeader : InternalSessionChannelHandler<ProtoService.AllianceChangeLeaderResponse>
        {
            public ChangeLeader(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnAllianceLeaderChanged;

            protected override async Task HandleMessage(ProtoService.AllianceChangeLeaderResponse res)
            {
                if (res.Code != 0)
                {
                    return;
                }

                _server.GuildManager.StoreAlliance(res.AllianceDto);

                var allMembers = res.AllianceDto.Guilds.SelectMany(x => x.Members).Select(x => x.Id);
                await _server.SendToPlayersAsync(allMembers, async chr =>
                {
                    if (chr.Id == res.Request.MasterId)
                    {
                        chr.AllianceRank = 2;
                    }
                    else if (chr.Id == res.Request.PlayerId)
                    {
                        chr.AllianceRank = 1;
                    }
                    await chr.SendPacket(GuildPackets.GetGuildAlliances(res.AllianceDto));
                    await chr.Notice("'" + res.NewLeaderName + "' has been appointed as the new head of this Alliance.");
                });
            }

            protected override ProtoService.AllianceChangeLeaderResponse Parse(ByteString data) => ProtoService.AllianceChangeLeaderResponse.Parser.ParseFrom(data);
        }
    }
}
