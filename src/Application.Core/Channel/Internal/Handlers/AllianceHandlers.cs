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

            protected override void HandleMessage(CreateAllianceResponse res)
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
                if (res.Code != 0)
                {
                    return;
                }

                _server.Broadcast(w =>
                {
                    foreach (var item in res.AllMembers)
                    {
                        w.getPlayerStorage().GetCharacterActor(item)?.Send(async m =>
                            {
                                var p = m.getCharacterById(item);
                                if (p != null)
                                {
                                    await p.SendPacket(GuildPackets.sendShowInfo(res.AllianceId, res.Request.MasterId));
                                }
                            });
                    }
                });
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
                if (res.Code != 0)
                {
                    return;
                }

                _server.Broadcast(w =>
                {
                    foreach (var memberId in res.AllMembers)
                    {
                        w.getPlayerStorage().GetCharacterActor(memberId)?.Send(async m =>
                            {
                                var p = m.getCharacterById(memberId);
                                if (p != null)
                                {
                                    await p.SendPacket(GuildPackets.allianceNotice(res.AllianceId, res.Request.Notice));
                                    await p.dropMessage(5, "* Alliance Notice : " + res.Request.Notice);
                                }
                            });
                    }
                });
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
                if (res.Code != 0)
                {
                    return;
                }

                _server.Broadcast(w =>
                {
                    foreach (var member in res.AllianceDto.Guilds.SelectMany(x => x.Members))
                    {
                        w.getPlayerStorage().GetCharacterActor(member.Id)?.Send(async m =>
                            {
                                var chr = m.getCharacterById(member.Id);
                                if (chr != null)
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
                                }
                            });
                    }

                });

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
                if (res.Code != 0)
                {
                    return;
                }

                _server.Broadcast(w =>
                {
                    foreach (var member in res.AllianceDto.Guilds.SelectMany(x => x.Members))
                    {
                        w.getPlayerStorage().GetCharacterActor(member.Id)?.Send(async m =>
                            {
                                var chr = m.getCharacterById(member.Id);
                                if (chr != null)
                                {
                                    await chr.SendPacket(GuildPackets.RemoveGuildFromAlliance(res.AllianceDto, res.GuildDto));
                                    await chr.SendPacket(GuildPackets.GetGuildAlliances(res.AllianceDto));
                                    await chr.SendPacket(GuildPackets.allianceNotice(res.AllianceId, res.AllianceDto.Notice));
                                    await chr.Notice("[" + res.GuildDto.Name + "] guild has left the union.");
                                }
                            });
                    }

                    foreach (var guildMember in res.GuildDto.Members)
                    {
                        w.getPlayerStorage().GetCharacterActor(guildMember.Id)?.Send(async m =>
                            {
                                var chr = m.getCharacterById(guildMember.Id);
                                if (chr != null)
                                {
                                    await chr.SendPacket(GuildPackets.RemoveGuildFromAlliance(res.AllianceDto, res.GuildDto));
                                    await chr.SendPacket(GuildPackets.disbandAlliance(res.AllianceId));
                                }
                            });
                    }

                });

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
                if (res.Code != 0)
                {
                    return;
                }

                _server.Broadcast(w =>
                {
                    foreach (var member in res.AllianceDto.Guilds.SelectMany(x => x.Members))
                    {
                        w.getPlayerStorage().GetCharacterActor(member.Id)?.Send(async m =>
                            {
                                var chr = m.getCharacterById(member.Id);
                                if (chr != null)
                                {
                                    await chr.SendPacket(GuildPackets.RemoveGuildFromAlliance(res.AllianceDto, res.GuildDto));
                                    await chr.SendPacket(GuildPackets.GetGuildAlliances(res.AllianceDto));
                                    await chr.SendPacket(GuildPackets.allianceNotice(res.AllianceId, res.AllianceDto.Notice));
                                    await chr.Notice("[" + res.GuildDto.Name + "] guild has been expelled from the union.");
                                }
                            });
                    }

                    foreach (var guildMember in res.GuildDto.Members)
                    {
                        w.getPlayerStorage().GetCharacterActor(guildMember.Id)?.Send(async m =>
                            {
                                var chr = m.getCharacterById(guildMember.Id);
                                if (chr != null)
                                {
                                    await chr.SendPacket(GuildPackets.RemoveGuildFromAlliance(res.AllianceDto, res.GuildDto));
                                    await chr.SendPacket(GuildPackets.disbandAlliance(res.AllianceId));
                                }
                            });
                    }
                });

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
                if (res.Code != 0)
                {
                    return;
                }

                _server.Broadcast(w =>
                {
                    foreach (var member in res.AllianceDto.Guilds.SelectMany(x => x.Members))
                    {
                        w.getPlayerStorage().GetCharacterActor(member.Id)?.Send(async m =>
                            {
                                var chr = m.getCharacterById(member.Id);
                                if (chr != null)
                                {
                                    if (chr.Id == res.Request.MasterId)
                                    {
                                        await chr.SendPacket(GuildPackets.UpdateAllianceInfo(res.AllianceDto));
                                    }
                                }
                            });
                    }
                });
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
                if (res.Code != 0)
                {
                    return;
                }

                _server.Broadcast(w =>
                {
                    foreach (var memberId in res.AllMembers)
                    {
                        w.getPlayerStorage().GetCharacterActor(memberId)?.Send(async m =>
                            {
                                var p = m.getCharacterById(memberId);
                                if (p != null)
                                {
                                    await p.SendPacket(GuildPackets.disbandAlliance(res.AllianceId));
                                    p.AllianceRank = 5;
                                }
                            });
                    }
                });
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
                if (res.Code != 0)
                {
                    return;
                }


                _server.Broadcast(w =>
                {
                    foreach (var member in res.AllianceDto.Guilds.SelectMany(x => x.Members))
                    {
                        w.getPlayerStorage().GetCharacterActor(member.Id)?.Send(async m =>
                            {
                                var p = m.getCharacterById(member.Id);
                                if (p != null)
                                {
                                    await p.SendPacket(GuildPackets.GetGuildAlliances(res.AllianceDto));
                                    p.AllianceRank = res.NewRank;
                                }
                            });
                    }
                });
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
                if (res.Code != 0)
                {
                    return;
                }

                _server.Broadcast(w =>
                {
                    foreach (var memberId in res.AllMembers)
                    {
                        w.getPlayerStorage().GetCharacterActor(memberId)?.Send(async m =>
                            {
                                var p = m.getCharacterById(memberId);
                                if (p != null)
                                {
                                    await p.SendPacket(GuildPackets.changeAllianceRankTitle(res.AllianceId, res.Request.RankTitles.ToArray()));
                                }
                            });
                    }
                });

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
                if (res.Code != 0)
                {
                    return;
                }

                _server.Broadcast(w =>
                {
                    foreach (var member in res.AllianceDto.Guilds.SelectMany(x => x.Members))
                    {
                        var mapActor = w.getPlayerStorage().GetCharacterActor(member.Id);
                        if (mapActor != null)
                        {
                            mapActor.Send(async m =>
                            {
                                var chr = m.getCharacterById(member.Id);
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
                                    await chr.SendPacket(GuildPackets.GetGuildAlliances(res.AllianceDto));
                                    await chr.Notice("'" + res.NewLeaderName + "' has been appointed as the new head of this Alliance.");
                                }
                            });
                        }
                    }
                });
                _server.GuildManager.StoreAlliance(res.AllianceDto);
            }

            protected override AllianceChangeLeaderResponse Parse(ByteString data) => AllianceChangeLeaderResponse.Parser.ParseFrom(data);
        }
    }
}
