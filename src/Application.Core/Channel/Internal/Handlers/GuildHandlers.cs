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
                case GuildUpdateResult.GuildCapacityFull:
                    return "Your guild already reached the maximum capacity of players.";
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

            protected override async Task HandleMessage(CreateGuildResponse res)
            {
                var resCode = (GuildUpdateResult)res.Code;
                if (resCode == GuildUpdateResult.Success)
                {
                    _server.GuildManager.StoreGuild(res.GuildDto);

                    var members = res.GuildDto.Members.Select(x => x.Id);
                    await _server.SendToPlayersAsync(members, async chr =>
                    {
                        if (res.GuildDto.Leader == chr.Id)
                        {
                            chr.GuildRank = 1;
                            await chr.Popup("You have successfully created a Guild.");
                        }
                        else
                        {
                            chr.GuildRank = 2;
                            await chr.Popup("You have successfully cofounded a Guild.");
                        }

                        await chr.SendPacket(GuildPackets.ShowGuildInfo(res.GuildDto));

                        await chr.BroadcastMap(GuildPackets.guildNameChanged(chr.Id, res.GuildDto.Name), chr.Id);
                        await chr.BroadcastMap(GuildPackets.guildMarkChanged(chr.Id, res.GuildDto.LogoBg, res.GuildDto.LogoBgColor, res.GuildDto.Logo, res.GuildDto.LogoColor), chr.Id);
                    });
                }
                else
                {
                    await _server.SendToPlayerAsync(res.Request.LeaderId, async masterChr =>
                    {
                        var msg = GuildHandlers.GetErrorMessage(resCode);
                        if (msg != null)
                        {
                            // 返还
                            await masterChr.GainMeso(YamlConfig.config.server.CREATE_GUILD_COST);
                            await masterChr.Popup(msg);
                        }
                    });
                }
            }

            protected override CreateGuildResponse Parse(ByteString data) => CreateGuildResponse.Parser.ParseFrom(data);
        }

        public class GuildMemberServerChangedHandler : InternalSessionChannelHandler<GuildProto.GuildMemberServerChangedResponse>
        {
            public GuildMemberServerChangedHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnGuildMemberLoginOff;

            protected override async Task HandleMessage(GuildMemberServerChangedResponse res)
            {
                await _server.SendToPlayersAsync(res.AllMembers, async chr =>
                {
                    if (chr.Id != res.MemberId)
                    {
                        // 同家族
                        if (chr.GuildId == res.GuildId)
                        {
                            await chr.SendPacket(GuildPackets.guildMemberOnline(res.GuildId, res.MemberId, res.MemberChanel > 0));
                        }
                        // 同联盟
                        await chr.SendPacket(GuildPackets.allianceMemberOnline(res.AllianceId, res.GuildId, res.MemberId, res.MemberChanel > 0));
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

            protected override async Task HandleMessage(GuildMemberUpdateResponse res)
            {
                await _server.SendToPlayersAsync(res.AllMembers, async chr =>
                {
                    if (chr.GuildId == res.GuildId)
                    {
                        if (res.Type == 0)
                        {
                            await chr.SendPacket(PacketCreator.levelUpMessage(2, res.MemberLevel, res.MemberName));
                        }
                        else
                        {
                            await chr.SendPacket(PacketCreator.jobMessage(0, res.MemberJob, res.MemberName));
                        }
                        await chr.SendPacket(GuildPackets.guildMemberLevelJobUpdate(res.GuildId, res.MemberId, res.MemberLevel, res.MemberJob));
                    }
                    await chr.SendPacket(GuildPackets.updateAllianceJobLevel(res.AllianceId, res.GuildId, res.MemberId, res.MemberLevel, res.MemberJob));
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

            protected override async Task HandleMessage(UpdateGuildNoticeResponse res)
            {
                var resCode = (GuildUpdateResult)res.Code;
                if (resCode != GuildUpdateResult.Success)
                {
                    return;
                }

                _server.GuildManager.ClearGuildCache(res.GuildId);
                await _server.SendToPlayersAsync(res.GuildMembers, async chr =>
                 {
                     await chr.SendPacket(GuildPackets.guildNotice(res.GuildId, res.Request.Notice));
                 });
            }

            protected override UpdateGuildNoticeResponse Parse(ByteString data) => UpdateGuildNoticeResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildGpUpdateHandler : InternalSessionChannelHandler<GuildProto.UpdateGuildGPResponse>
        {
            public InvokeGuildGpUpdateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnGuildGpUpdate;

            protected override async Task HandleMessage(UpdateGuildGPResponse res)
            {
                var resCode = (GuildUpdateResult)res.Code;
                if (resCode != GuildUpdateResult.Success)
                {
                    return;
                }

                await _server.SendToPlayersAsync(res.GuildMembers, async chr =>
                {
                    await chr.SendPacket(GuildPackets.updateGP(res.GuildId, res.GuildGP));
                    if (res.Request.Gp > 0)
                    {
                        await chr.SendPacket(PacketCreator.getGPMessage(res.Request.Gp));
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

            protected override async Task HandleMessage(UpdateGuildCapacityResponse res)
            {
                var resCode = (GuildUpdateResult)res.Code;
                if (resCode == GuildUpdateResult.Success)
                {
                    _server.GuildManager.ClearGuildCache(res.GuildId);

                    await _server.SendToPlayersAsync([res.Request.MasterId, .. res.GuildMembers], async chr =>
                    {
                        await chr.SendPacket(GuildPackets.guildCapacityChange(res.GuildId, res.GuildCapacity));
                    });
                }
                else
                {
                    await _server.SendToPlayerAsync(res.Request.MasterId, async masterChr =>
                    {
                        var msg = GuildHandlers.GetErrorMessage(resCode);
                        if (msg != null)
                        {
                            // 预扣返还
                            await masterChr.GainMeso(res.Request.Cost);
                            await masterChr.Popup(msg);
                        }
                    });
                }
            }

            protected override UpdateGuildCapacityResponse Parse(ByteString data) => UpdateGuildCapacityResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildEmblemUpdateHandler : InternalSessionChannelHandler<GuildProto.UpdateGuildEmblemResponse>
        {
            public InvokeGuildEmblemUpdateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnGuildEmblemUpdate;

            protected override async Task HandleMessage(UpdateGuildEmblemResponse res)
            {
                var resCode = (GuildUpdateResult)res.Code;
                if (resCode == GuildUpdateResult.Success)
                {
                    _server.GuildManager.ClearGuildCache(res.GuildId);
                    _server.GuildManager.StoreAlliance(res.AllianceDto);

                    var guildDto = res.AllianceDto.Guilds.FirstOrDefault(x => x.GuildId == res.GuildId)!;
                    await _server.SendToPlayersAsync([res.Request.MasterId, .. res.AllMembers], async chr =>
                    {
                        if (chr.Id == res.Request.MasterId)
                        {
                            await chr.GainMeso(YamlConfig.config.server.CHANGE_EMBLEM_COST);
                        }

                        if (chr.GuildId == res.GuildId)
                        {
                            await chr.SendPacket(GuildPackets.guildEmblemChange(res.GuildId, (short)res.Request.LogoBg, (byte)res.Request.LogoBgColor, (short)res.Request.Logo, (byte)res.Request.LogoColor));
                        }

                        if (res.AllianceDto != null)
                        {
                            await chr.SendPacket(GuildPackets.GetGuildAlliances(res.AllianceDto));
                        }

                        await chr.BroadcastMap(GuildPackets.guildMarkChanged(chr.Id, guildDto.LogoBg, guildDto.LogoBgColor, guildDto.Logo, guildDto.LogoColor), chr.Id);
                    });
                }
                else
                {
                    await _server.SendToPlayerAsync(res.Request.MasterId, async masterChr =>
                    {
                        var msg = GuildHandlers.GetErrorMessage(resCode);
                        if (msg != null)
                        {
                            await masterChr.Popup(msg);
                        }
                    });
                }
            }

            protected override UpdateGuildEmblemResponse Parse(ByteString data) => UpdateGuildEmblemResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildRankTitleUpdateHandler : InternalSessionChannelHandler<GuildProto.UpdateGuildRankTitleResponse>
        {
            public InvokeGuildRankTitleUpdateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnGuildRankTitleUpdate;

            protected override async Task HandleMessage(UpdateGuildRankTitleResponse res)
            {
                var resCode = (GuildUpdateResult)res.Code;
                if (resCode != GuildUpdateResult.Success)
                {
                    return;
                }

                _server.GuildManager.ClearGuildCache(res.GuildId);

                await _server.SendToPlayersAsync(res.GuildMembers, async chr =>
                {
                    await chr.SendPacket(GuildPackets.rankTitleChange(res.GuildId, res.Request.RankTitles.ToArray()));
                });
            }

            protected override UpdateGuildRankTitleResponse Parse(ByteString data) => UpdateGuildRankTitleResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildMemberRankUpdateHandler : InternalSessionChannelHandler<GuildProto.UpdateGuildMemberRankResponse>
        {
            public InvokeGuildMemberRankUpdateHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnGuildRankChanged;

            protected override async Task HandleMessage(UpdateGuildMemberRankResponse res)
            {
                var resCode = (GuildUpdateResult)res.Code;
                if (resCode != GuildUpdateResult.Success)
                {
                    return;
                }

                _server.GuildManager.ClearGuildCache(res.GuildId);

                await _server.SendToPlayersAsync(res.GuildMembers, async chr =>
                {
                    if (chr.Id == res.Request.TargetPlayerId)
                    {
                        chr.GuildRank = res.Request.NewRank;
                    }
                    await chr.SendPacket(GuildPackets.changeRank(res.GuildId, res.Request.TargetPlayerId, res.Request.NewRank));
                });
            }

            protected override UpdateGuildMemberRankResponse Parse(ByteString data) => UpdateGuildMemberRankResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildMemberJoinHandler : InternalSessionChannelHandler<GuildProto.JoinGuildResponse>
        {
            public InvokeGuildMemberJoinHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnPlayerJoinGuild;

            protected override async Task HandleMessage(JoinGuildResponse res)
            {
                var resCode = (GuildUpdateResult)res.Code;
                if (resCode == GuildUpdateResult.Success)
                {
                    _server.GuildManager.StoreGuild(res.GuildDto);
                    _server.GuildManager.StoreAlliance(res.AllianceDto);

                    var newMember = res.GuildDto.Members.FirstOrDefault(x => x.Id == res.Request.PlayerId)!;
                    await _server.SendToPlayersAsync(res.AllMembers, async chr =>
                    {
                        if (chr.Id == res.Request.PlayerId)
                        {
                            chr.GuildRank = newMember.GuildRank;
                            await chr.SendPacket(GuildPackets.ShowGuildInfo(res.GuildDto));
                            await chr.BroadcastMap(GuildPackets.guildNameChanged(chr.Id, res.GuildDto.Name), chr.Id);
                            await chr.BroadcastMap(GuildPackets.guildMarkChanged(chr.Id, res.GuildDto.LogoBg, res.GuildDto.LogoBgColor, res.GuildDto.Logo, res.GuildDto.LogoColor), chr.Id);
                        }
                        else if (chr.GuildId == res.Request.GuildId)
                        {
                            await chr.SendPacket(GuildPackets.newGuildMember(res.Request.GuildId,
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
                                await chr.SendPacket(GuildPackets.UpdateAllianceInfo(res.AllianceDto));
                                await chr.SendPacket(GuildPackets.allianceNotice(res.AllianceDto.AllianceId, res.AllianceDto.Notice));
                            }
                        }
                    });
                }
                else
                {
                    await _server.SendToPlayerAsync(res.Request.PlayerId, async masterChr =>
                    {
                        var msg = GuildHandlers.GetErrorMessage(resCode);
                        if (msg != null)
                        {
                            await masterChr.Popup(msg);
                        }
                    });
                }
            }

            protected override JoinGuildResponse Parse(ByteString data) => JoinGuildResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildMemberLeaveHandler : InternalSessionChannelHandler<GuildProto.LeaveGuildResponse>
        {
            public InvokeGuildMemberLeaveHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnPlayerLeaveGuild;

            protected override async Task HandleMessage(LeaveGuildResponse res)
            {
                var resCode = (GuildUpdateResult)res.Code;
                if (resCode == GuildUpdateResult.Success)
                {
                    _server.GuildManager.ClearGuildCache(res.GuildId);
                    _server.GuildManager.StoreAlliance(res.AllianceDto);

                    await _server.SendToPlayersAsync([res.Request.PlayerId, .. res.AllLeftMembers], async chr =>
                    {
                        if (res.Request.PlayerId == chr.Id)
                        {
                            await chr.SendPacket(GuildPackets.updateGP(res.GuildId, 0));
                            await chr.SendPacket(GuildPackets.ShowGuildInfo(null));

                            await chr.BroadcastMap(GuildPackets.guildNameChanged(chr.Id, ""), chr.Id);
                        }

                        if (res.AllLeftMembers.Contains(chr.Id))
                        {
                            if (chr.GuildId == res.GuildId)
                            {
                                await chr.SendPacket(GuildPackets.memberLeft(res.GuildId, res.Request.PlayerId, res.MasterName, false));
                            }
                            else
                            {
                                if (res.AllianceDto != null)
                                {
                                    await chr.SendPacket(GuildPackets.UpdateAllianceInfo(res.AllianceDto));
                                    await chr.SendPacket(GuildPackets.allianceNotice(res.AllianceDto.AllianceId, res.AllianceDto.Notice));
                                }
                            }
                        }
                    });
                }
                else
                {
                    await _server.SendToPlayerAsync(res.Request.PlayerId, async masterChr =>
                    {
                        var msg = GuildHandlers.GetErrorMessage(resCode);
                        if (msg != null)
                        {
                            await masterChr.Popup(msg);
                        }
                    });
                }
            }

            protected override LeaveGuildResponse Parse(ByteString data) => LeaveGuildResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildExpelMemberHandler : InternalSessionChannelHandler<GuildProto.ExpelFromGuildResponse>
        {
            public InvokeGuildExpelMemberHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnGuildExpelMember;

            protected override async Task HandleMessage(ExpelFromGuildResponse res)
            {
                var resCode = (GuildUpdateResult)res.Code;
                if (resCode == GuildUpdateResult.Success)
                {
                    _server.GuildManager.ClearGuildCache(res.GuildId);
                    _server.GuildManager.StoreAlliance(res.AllianceDto);

                    await _server.SendToPlayersAsync([res.Request.TargetPlayerId, .. res.AllLeftMembers], async chr =>
                    {
                        if (res.Request.TargetPlayerId == chr.Id)
                        {
                            await chr.SendPacket(GuildPackets.updateGP(res.GuildId, 0));
                            await chr.SendPacket(GuildPackets.ShowGuildInfo(null));

                            await chr.BroadcastMap(GuildPackets.guildNameChanged(chr.Id, ""), chr.Id);
                        }

                        if (res.AllLeftMembers.Contains(chr.Id))
                        {
                            if (chr.GuildId == res.GuildId)
                            {
                                await chr.SendPacket(GuildPackets.memberLeft(res.GuildId, res.Request.TargetPlayerId, res.TargetName, true));
                            }
                            else
                            {
                                if (res.AllianceDto != null)
                                {
                                    await chr.SendPacket(GuildPackets.UpdateAllianceInfo(res.AllianceDto));
                                    await chr.SendPacket(GuildPackets.allianceNotice(res.AllianceDto.AllianceId, res.AllianceDto.Notice));
                                }
                            }
                        }
                    });
                }
                else
                {
                    await _server.SendToPlayerAsync(res.Request.MasterId, async masterChr =>
                    {
                        if (resCode == GuildUpdateResult.MasterRankFail)
                        {
                            await masterChr.Popup("权限不足");
                        }
                    });
                }
            }

            protected override ExpelFromGuildResponse Parse(ByteString data) => ExpelFromGuildResponse.Parser.ParseFrom(data);
        }

        public class InvokeGuildDisbandHandler : InternalSessionChannelHandler<GuildProto.GuildDisbandResponse>
        {
            public InvokeGuildDisbandHandler(WorldChannelServer server) : base(server)
            {
            }
            public override int MessageId => (int)ChannelRecvCode.OnGuildDisband;

            protected override async Task HandleMessage(GuildDisbandResponse res)
            {
                if (res.Code != 0)
                {
                    return;
                }

                _server.GuildManager.ClearGuildCache(res.GuildId);
                _server.GuildManager.StoreAlliance(res.AllianceDto);

                await _server.SendToPlayersAsync(res.AllMembers, async chr =>
                {
                    if (chr.GuildId == res.GuildId)
                    {
                        await chr.SendPacket(GuildPackets.updateGP(res.GuildId, 0));
                        await chr.SendPacket(GuildPackets.ShowGuildInfo(null));

                        await chr.BroadcastMap(GuildPackets.guildNameChanged(chr.Id, ""), chr.Id);
                    }
                    else
                    {
                        if (res.AllianceDto != null)
                        {
                            await chr.SendPacket(GuildPackets.UpdateAllianceInfo(res.AllianceDto));
                            await chr.SendPacket(GuildPackets.allianceNotice(res.AllianceDto.AllianceId, res.AllianceDto.Notice));
                        }
                    }
                });
            }

            protected override GuildDisbandResponse Parse(ByteString data) => GuildDisbandResponse.Parser.ParseFrom(data);
        }
    }
}
