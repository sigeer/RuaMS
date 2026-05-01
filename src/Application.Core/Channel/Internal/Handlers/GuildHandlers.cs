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
                _server.Broadcast(w =>
                {
                    if (res.Code != 0)
                    {
                        w.getPlayerStorage().GetCharacterActor(res.Request.LeaderId)?
                        .Send(m =>
                        {
                            var masterChr = m.getCharacterById(res.Request.LeaderId);
                            if (masterChr != null)
                            {
                                var msg = GuildHandlers.GetErrorMessage((GuildUpdateResult)res.Code);
                                if (msg != null)
                                {
                                    masterChr.Popup(msg);
                                }
                                masterChr.GainMeso(YamlConfig.config.server.CREATE_GUILD_COST);
                            }
                        });
                        return;
                    }

                    foreach (var member in res.GuildDto.Members)
                    {
                        w.getPlayerStorage().GetCharacterActor(member.Id)?
                        .Send(m =>
                        {
                            var chr = m.getCharacterById(member.Id);

                            if (chr != null)
                            {
                                if (res.GuildDto.Leader == chr.Id)
                                {
                                    chr.GuildRank = 1;
                                    chr.Popup("You have successfully created a Guild.");
                                }
                                else
                                {
                                    chr.GuildRank = 2;
                                    chr.Popup("You have successfully cofounded a Guild.");
                                }
                                chr.sendPacket(GuildPackets.ShowGuildInfo(res.GuildDto));

                                chr.getMap().broadcastPacket(chr, GuildPackets.guildNameChanged(chr.Id, res.GuildDto.Name));
                                chr.getMap().broadcastPacket(chr, GuildPackets.guildMarkChanged(chr.Id, res.GuildDto.LogoBg, res.GuildDto.LogoBgColor, res.GuildDto.Logo, res.GuildDto.LogoColor));
                            }
                        });
                    }
                });
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
                _server.Broadcast(w =>
                {
                    foreach (var guild in res.AllMembers)
                    {
                        if (guild != res.MemberId)
                        {
                            w.getPlayerStorage().GetCharacterActor(guild)?.Send(m =>
                                {
                                    var chr = m.getCharacterById(guild);
                                    if (chr != null)
                                    {
                                        if (chr.GuildId == res.GuildId)
                                        {
                                            chr.sendPacket(GuildPackets.guildMemberOnline(res.GuildId, res.MemberId, res.MemberChanel > 0));
                                        }
                                        chr.sendPacket(GuildPackets.allianceMemberOnline(res.AllianceId, res.GuildId, res.MemberId, res.MemberChanel > 0));
                                    }
                                });
                        }
                    }
                });

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
                _server.Broadcast(w =>
                {
                    foreach (var guild in res.AllMembers)
                    {
                        var actor = w.getPlayerStorage().GetCharacterActor(guild);
                        if (actor != null)
                            actor.Send(m =>
                            {
                                var chr = m.getCharacterById(guild);
                                if (chr != null)
                                {
                                    if (chr.GuildId == res.GuildId)
                                    {
                                        if (res.Type == 0)
                                        {
                                            chr.sendPacket(PacketCreator.levelUpMessage(2, res.MemberLevel, res.MemberName));
                                        }
                                        else
                                        {
                                            chr.sendPacket(PacketCreator.jobMessage(0, res.MemberJob, res.MemberName));
                                        }
                                        chr.sendPacket(GuildPackets.guildMemberLevelJobUpdate(res.GuildId, res.MemberId, res.MemberLevel, res.MemberJob));
                                    }
                                    chr.sendPacket(GuildPackets.updateAllianceJobLevel(res.AllianceId, res.GuildId, res.MemberId, res.MemberLevel, res.MemberJob));
                                }
                            });
                    }
                });
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

                _server.Broadcast(w =>
                {
                    foreach (var memberId in res.GuildMembers)
                    {
                        var actor = w.getPlayerStorage().GetCharacterActor(memberId);
                        if (actor != null)
                            actor.Send(m =>
                            {
                                var chr = m.getCharacterById(memberId);
                                if (chr != null)
                                {
                                    chr.sendPacket(GuildPackets.guildNotice(res.GuildId, res.Request.Notice));
                                }
                            });
                    }
                });

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

                _server.Broadcast(w =>
                {
                    foreach (var memberId in res.GuildMembers)
                    {
                        w.getPlayerStorage().GetCharacterActor(memberId)?.Send(m =>
                        {
                            var chr = m.getCharacterById(memberId);
                            if (chr != null)
                            {
                                chr.sendPacket(GuildPackets.updateGP(res.GuildId, res.GuildGP));
                                if (res.Request.Gp > 0)
                                {
                                    chr.sendPacket(PacketCreator.getGPMessage(res.Request.Gp));
                                }
                            }
                        });

                    }
                });

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
                _server.Broadcast(w =>
                {
                    var resCode = (GuildUpdateResult)res.Code;
                    if (resCode != GuildUpdateResult.Success)
                    {
                        w.getPlayerStorage().GetCharacterActor(res.Request.MasterId)?.Send(m =>
                        {
                            var masterChr = m.getCharacterById(res.Request.MasterId);
                            if (masterChr != null)
                            {
                                if (resCode == GuildUpdateResult.GuildFull)
                                {
                                    masterChr.Popup("Your guild already reached the maximum capacity of players.");
                                }
                                masterChr.GainMeso(res.Request.Cost);
                            }
                        });
                        return;
                    }


                    foreach (var memberId in res.GuildMembers)
                    {
                        w.getPlayerStorage().GetCharacterActor(memberId)?.Send(m =>
                        {
                            var chr = m.getCharacterById(memberId);
                            if (chr != null)
                            {
                                chr.sendPacket(GuildPackets.guildCapacityChange(res.GuildId, res.GuildCapacity));
                            }
                        });
                    }
                });

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
                _server.Broadcast(w =>
                {
                    var resCode = (GuildUpdateResult)res.Code;
                    if (resCode != GuildUpdateResult.Success)
                    {
                        w.getPlayerStorage().GetCharacterActor(res.Request.MasterId)
                        ?.Send(m =>
                        {
                            m.getCharacterById(res.Request.MasterId)?.GainMeso(YamlConfig.config.server.CHANGE_EMBLEM_COST);
                        });
                        return;
                    }

                    var guildDto = res.AllianceDto.Guilds.FirstOrDefault(x => x.GuildId == res.GuildId)!;
                    foreach (var memberId in res.AllMembers)
                    {
                        w.getPlayerStorage().GetCharacterActor(memberId)?
                        .Send(m =>
                        {
                            var chr = m.getCharacterById(memberId);
                            if (chr != null)
                            {
                                if (chr.GuildId == res.GuildId)
                                {
                                    chr.sendPacket(GuildPackets.guildEmblemChange(res.GuildId, (short)res.Request.LogoBg, (byte)res.Request.LogoBgColor, (short)res.Request.Logo, (byte)res.Request.LogoColor));
                                }

                                if (res.AllianceDto != null)
                                {
                                    chr.sendPacket(GuildPackets.GetGuildAlliances(res.AllianceDto));
                                }
                                chr.MapModel.broadcastPacket(chr, GuildPackets.guildMarkChanged(chr.Id, guildDto.LogoBg, guildDto.LogoBgColor, guildDto.Logo, guildDto.LogoColor));
                            }
                        });

                    }
                });

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

                _server.Broadcast(w =>
                {
                    foreach (var memberId in res.GuildMembers)
                    {
                        var actor = w.getPlayerStorage().GetCharacterActor(memberId);
                        if (actor != null)
                            actor.Send(m =>
                            {
                                var chr = m.getCharacterById(memberId);
                                if (chr != null)
                                {
                                    chr.sendPacket(GuildPackets.rankTitleChange(res.GuildId, res.Request.RankTitles.ToArray()));
                                }
                            });
                    }
                });

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

                _server.Broadcast(w =>
                {
                    foreach (var memberId in res.GuildMembers)
                    {
                        var actor = w.getPlayerStorage().GetCharacterActor(memberId);
                        if (actor != null)
                            actor.Send(m =>
                            {
                                var chr = m.getCharacterById(memberId);
                                if (chr != null)
                                {
                                    if (chr.Id == res.Request.TargetPlayerId)
                                    {
                                        chr.GuildRank = res.Request.NewRank;
                                    }
                                    chr.sendPacket(GuildPackets.changeRank(res.GuildId, res.Request.TargetPlayerId, res.Request.NewRank));
                                }
                            });
                    }
                });

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
                _server.Broadcast(w =>
                {
                    var resCode = (GuildUpdateResult)res.Code;
                    if (resCode != GuildUpdateResult.Success)
                    {
                        w.getPlayerStorage().GetCharacterActor(res.Request.PlayerId)?
                           .Send(m =>
                           {
                               var masterChr = m.getCharacterById(res.Request.PlayerId);
                               if (masterChr != null)
                               {
                                   if (resCode == GuildUpdateResult.GuildFull)
                                   {
                                       masterChr.dropMessage(1, "The guild you are trying to join is already full.");
                                   }
                               }
                           });
                        return;
                    }

                    var newMember = res.GuildDto.Members.FirstOrDefault(x => x.Id == res.Request.PlayerId)!;
                    foreach (var memberId in res.AllMembers)
                    {
                        w.getPlayerStorage().GetCharacterActor(memberId)?.Send(m =>
                            {
                                var chr = m.getCharacterById(memberId);
                                if (chr != null)
                                {
                                    if (chr.Id == newMember.Id)
                                    {
                                        chr.GuildRank = newMember.GuildRank;
                                        chr.sendPacket(GuildPackets.ShowGuildInfo(res.GuildDto));
                                        chr.getMap().broadcastPacket(chr, GuildPackets.guildNameChanged(chr.Id, res.GuildDto.Name));
                                        chr.getMap().broadcastPacket(chr, GuildPackets.guildMarkChanged(chr.Id, res.GuildDto.LogoBg, res.GuildDto.LogoBgColor, res.GuildDto.Logo, res.GuildDto.LogoColor));
                                    }
                                    else if (chr.GuildId == res.Request.GuildId)
                                    {
                                        chr.sendPacket(GuildPackets.newGuildMember(res.Request.GuildId,
                                            newMember.Id,
                                            newMember.Name,
                                            newMember.Job,
                                            newMember.Level,
                                            newMember.GuildRank,
                                            newMember.Channel));
                                    }
                                    else
                                    {
                                        if (res.AllianceDto != null)
                                        {
                                            chr.sendPacket(GuildPackets.UpdateAllianceInfo(res.AllianceDto));
                                            chr.sendPacket(GuildPackets.allianceNotice(res.AllianceDto.AllianceId, res.AllianceDto.Notice));
                                        }
                                    }
                                }
                            });
                    }
                });

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
                _server.Broadcast(w =>
                {
                    var resCode = (GuildUpdateResult)res.Code;
                    var masterActor = w.getPlayerStorage().GetCharacterActor(res.Request.PlayerId);
                    if (masterActor != null)
                        masterActor.Send(m =>
                        {
                            var masterChr = m.getCharacterById(res.Request.PlayerId);
                            if (resCode != GuildUpdateResult.Success)
                            {
                                if (masterChr != null)
                                {
                                    var msg = GuildHandlers.GetErrorMessage(resCode);
                                    if (msg != null)
                                    {
                                        masterChr.dropMessage(1, msg);
                                    }
                                }
                                return;
                            }
                            if (masterChr != null)
                            {
                                masterChr.sendPacket(GuildPackets.updateGP(res.GuildId, 0));
                                masterChr.sendPacket(GuildPackets.ShowGuildInfo(null));
                                masterChr.getMap().broadcastPacket(masterChr, GuildPackets.guildNameChanged(masterChr.Id, ""));
                            }
                        });

                    foreach (var memberId in res.AllLeftMembers)
                    {
                        var actor = w.getPlayerStorage().GetCharacterActor(memberId);
                        if (actor != null)
                            actor.Send(m =>
                            {
                                var chr = m.getCharacterById(memberId);
                                if (chr != null)
                                {
                                    if (chr.GuildId == res.GuildId)
                                    {
                                        chr.sendPacket(GuildPackets.memberLeft(res.GuildId, res.Request.PlayerId, res.MasterName, false));
                                    }
                                    else
                                    {
                                        if (res.AllianceDto != null)
                                        {
                                            chr.sendPacket(GuildPackets.UpdateAllianceInfo(res.AllianceDto));
                                            chr.sendPacket(GuildPackets.allianceNotice(res.AllianceDto.AllianceId, res.AllianceDto.Notice));
                                        }
                                    }
                                }
                            });
                    }
                });

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
                _server.Broadcast(w =>
                {
                    var resCode = (GuildUpdateResult)res.Code;
                    if (resCode != GuildUpdateResult.Success)
                    {
                        var masterChr = w.getPlayerStorage().GetCharacterClientById(res.Request.MasterId);
                        if (masterChr != null)
                        {
                            if (resCode == GuildUpdateResult.MasterRankFail)
                            {
                                masterChr.Popup("权限不足");
                            }
                        }
                        return;
                    }

                    w.getPlayerStorage().GetCharacterActor(res.Request.TargetPlayerId)?.Send(m =>
                    {
                        var targetChr = m.getCharacterById(res.Request.TargetPlayerId);

                        if (targetChr != null)
                        {
                            targetChr.sendPacket(GuildPackets.updateGP(res.GuildId, 0));
                            targetChr.sendPacket(GuildPackets.ShowGuildInfo(null));

                            targetChr.getMap().broadcastPacket(targetChr, GuildPackets.guildNameChanged(targetChr.Id, ""));
                        }
                    });


                    foreach (var memberId in res.AllLeftMembers)
                    {
                        w.getPlayerStorage().GetCharacterActor(memberId)?.Send(m =>
                        {
                            var chr = m.getCharacterById(memberId);
                            if (chr != null)
                            {
                                if (chr.GuildId == res.GuildId)
                                {
                                    chr.sendPacket(GuildPackets.memberLeft(res.GuildId, res.Request.TargetPlayerId, res.TargetName, true));
                                }
                                else
                                {
                                    if (res.AllianceDto != null)
                                    {
                                        chr.sendPacket(GuildPackets.UpdateAllianceInfo(res.AllianceDto));
                                        chr.sendPacket(GuildPackets.allianceNotice(res.AllianceDto.AllianceId, res.AllianceDto.Notice));
                                    }
                                }

                            }
                        });

                    }
                });

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
                if (res.Code != 0)
                {
                    return;
                }

                _server.Broadcast(w =>
                {
                    foreach (var memberId in res.AllMembers)
                    {
                        w.getPlayerStorage().GetCharacterActor(memberId)?
                        .Send(m =>
                        {
                            var chr = m.getCharacterById(memberId);
                            if (chr != null)
                            {
                                if (chr.GuildId == res.GuildId)
                                {
                                    chr.sendPacket(GuildPackets.updateGP(res.GuildId, 0));
                                    chr.sendPacket(GuildPackets.ShowGuildInfo(null));

                                    chr.getMap().broadcastPacket(chr, GuildPackets.guildNameChanged(chr.Id, ""));
                                }
                                else
                                {
                                    if (res.AllianceDto != null)
                                    {
                                        chr.sendPacket(GuildPackets.UpdateAllianceInfo(res.AllianceDto));
                                        chr.sendPacket(GuildPackets.allianceNotice(res.AllianceDto.AllianceId, res.AllianceDto.Notice));
                                    }
                                }
                            }
                        });

                    }
                });
                _server.GuildManager.ClearGuildCache(res.GuildId);
                _server.GuildManager.StoreAlliance(res.AllianceDto);
            }

            protected override GuildDisbandResponse Parse(ByteString data) => GuildDisbandResponse.Parser.ParseFrom(data);
        }
    }
}
