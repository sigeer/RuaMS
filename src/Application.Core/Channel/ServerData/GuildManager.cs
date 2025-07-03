using Application.Core.Game.Relation;
using Application.Core.ServerTransports;
using Application.Shared.Constants.Npc;
using Application.Shared.Invitations;
using AutoMapper;
using constants.game;
using Dto;
using Microsoft.Extensions.Logging;
using net.server.coordinator.matchchecker;
using net.server.guild;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Application.Core.Channel.ServerData
{
    public class GuildManager
    {
        private ConcurrentDictionary<int, Guild?> CachedData { get; set; } = new();
        ConcurrentDictionary<int, Alliance> _allianceData;

        readonly ILogger<GuildManager> _logger;
        readonly IMapper _mapper;
        readonly IChannelServerTransport _transport;
        readonly WorldChannelServer _serverContainer;
        public GuildManager(ILogger<GuildManager> logger, IMapper mapper, IChannelServerTransport transport, WorldChannelServer serverContainer)
        {
            _logger = logger;
            _mapper = mapper;
            _transport = transport;
            _serverContainer = serverContainer;
            _allianceData = new ConcurrentDictionary<int, Alliance>();
        }

        public bool CheckGuildName(string name)
        {
            if (name.Length < 3 || name.Length > 12)
            {
                return false;
            }
            for (int i = 0; i < name.Length; i++)
            {
                if (!char.IsLower(name.ElementAt(i)) && !char.IsUpper(name.ElementAt(i)))
                {
                    return false;
                }
            }
            return true;
        }

        public bool CheckAllianceName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Contains(" ") || name.Length > 12)
            {
                return false;
            }

            return _transport.CreateAllianceCheck(new CreateAllianceCheckRequest { Name = name }).IsValid;
        }

        public HashSet<IPlayer> getEligiblePlayersForGuild(IPlayer guildLeader)
        {
            HashSet<IPlayer> guildMembers = new();
            guildMembers.Add(guildLeader);

            MatchCheckerCoordinator mmce = guildLeader.getWorldServer().getMatchCheckerCoordinator();
            foreach (var chr in guildLeader.getMap().getAllPlayers())
            {
                if (chr.getParty() == null && chr.getGuild() == null && mmce.getMatchConfirmationLeaderid(chr.getId()) == -1)
                {
                    guildMembers.Add(chr);
                }
            }

            return guildMembers;
        }

        public void SendInvitation(IChannelClient c, string targetName)
        {
            _transport.SendInvitation(new Dto.CreateInviteRequest
            {
                Type = InviteTypes.Guild,
                FromId = c.OnlinedCharacter.Id,
                ToName = targetName,
            });
        }



        public void AnswerInvitation(IPlayer answer, int guildId, bool operation)
        {
            _transport.AnswerInvitation(new AnswerInviteRequest { Type = InviteTypes.Guild, MasterId = answer.Id, CheckKey = guildId, Ok = operation });
        }

        public Guild? CreateGuild(string guildName, int playerId, HashSet<IPlayer> members, Action failCallback)
        {
            var leader = members.FirstOrDefault(x => x.getId() == playerId);

            if (leader == null || !leader.isLoggedinWorld())
            {
                failCallback();
                return null;
            }
            members.Remove(leader);

            if (leader.getGuildId() > 0)
            {
                leader.dropMessage(1, "You cannot create a new Guild while in one.");
                failCallback();
                return null;
            }
            int partyid = leader.getPartyId();
            if (partyid == -1 || !leader.isPartyLeader())
            {
                leader.dropMessage(1, "You cannot establish the creation of a new Guild without leading a party.");
                failCallback();
                return null;
            }
            if (leader.getMapId() != MapId.GUILD_HQ)
            {
                leader.dropMessage(1, "You cannot establish the creation of a new Guild outside of the Guild Headquarters.");
                failCallback();
                return null;
            }
            foreach (var chr in members)
            {
                if (leader.getMap().getCharacterById(chr.getId()) == null)
                {
                    leader.dropMessage(1, "You cannot establish the creation of a new Guild if one of the members is not present here.");
                    failCallback();
                    return null;
                }
            }
            if (leader.getMeso() < YamlConfig.config.server.CREATE_GUILD_COST)
            {
                leader.dropMessage(1, "You do not have " + GameConstants.numberWithCommas(YamlConfig.config.server.CREATE_GUILD_COST) + " mesos to create a Guild.");
                failCallback();
                return null;
            }


            var remoteData = _transport.CreateGuild(guildName, playerId, members.Select(x => x.Id).ToArray()).Model;
            if (remoteData == null)
            {
                leader.sendPacket(GuildPackets.genericGuildMessage(0x23));
                failCallback();
                return null;
            }

            var guild = new Guild(_serverContainer, remoteData.GuildId);
            _mapper.Map(remoteData, guild);
            CachedData[guild.GuildId] = guild;

            leader.gainMeso(-YamlConfig.config.server.CREATE_GUILD_COST, true, false, true);

            leader.GuildId = guild.GuildId;
            guild.ChangeRank(leader.Id, 1);

            leader.sendPacket(GuildPackets.showGuildInfo(leader));
            leader.dropMessage(1, "You have successfully created a Guild.");

            foreach (var chr in members)
            {
                bool cofounder = chr.getPartyId() == partyid;

                chr.GuildId = guild.GuildId;
                chr.GuildRank = cofounder ? 2 : 5;
                chr.AllianceRank = 5;

                if (chr.isLoggedinWorld())
                {
                    chr.sendPacket(GuildPackets.showGuildInfo(chr));

                    if (cofounder)
                    {
                        chr.dropMessage(1, "You have successfully cofounded a Guild.");
                    }
                    else
                    {
                        chr.dropMessage(1, "You have successfully joined the new Guild.");
                    }
                }
            }

            guild.BroadcastDisplay();


            return guild;
        }

        public void OnPlayerJoinGuild(JoinGuildResponse data)
        {
            HandleGuildResponse(data.Code, data.Request.GuildId, data.Request.PlayerId,
                guild =>
                {
                    var result = guild.AddGuildMember(_mapper.Map<GuildMember>(data.Member));

                    if (result)
                        GetAllianceById(guild.AllianceId)?.updateAlliancePackets();

                    return result;
                },
                mc =>
                {
                    mc.sendPacket(GuildPackets.showGuildInfo(mc));
                },
                mc =>
                {
                    mc.dropMessage(1, "The guild you are trying to join is already full.");
                });
        }


        public void LeaveMember(IPlayer fromChr)
        {
            _transport.SendPlayerLeaveGuild(new LeaveGuildRequest { PlayerId = fromChr.Id });
        }

        public void OnPlayerLeaveGuild(LeaveGuildResponse data)
        {
            HandleGuildResponse(data.Code, data.GuildId, data.Request.PlayerId,
                guild =>
                {
                    var result = guild.LeaveGuild(data.Request.PlayerId);

                    if (result)
                        GetAllianceById(guild.AllianceId)?.updateAlliancePackets();

                    return result;
                },
                mc =>
                {
                    mc.sendPacket(GuildPackets.updateGP(mc.GuildId, 0));
                    mc.sendPacket(GuildPackets.showGuildInfo(null));
                });
        }

        public Guild? GetGuildById(int id)
        {
            if (CachedData.TryGetValue(id, out var d) && d != null)
                return d;

            var remoteData = _transport.GetGuild(id);
            if (remoteData == null || remoteData.Model == null)
            {
                return null;
            }
            var localData = new Guild(_serverContainer, id);
            _mapper.Map(remoteData.Model, localData);
            CachedData[localData.GuildId] = localData;
            return localData;
        }

        public void ExpelMember(IPlayer fromChr, int toId)
        {
            _transport.SendGuildExpelMember(new ExpelFromGuildRequest { MasterId = fromChr.Id, TargetPlayerId = toId });
        }

        public void OnGuildExpelMember(ExpelFromGuildResponse data)
        {
            HandleGuildResponse(data.Code, data.GuildId, data.Request.MasterId, guild =>
            {
                var result = guild.ExpelMember(data.Request.TargetPlayerId);
                var alliance = GetAllianceById(guild.AllianceId);
                alliance?.updateAlliancePackets();
                return result;
            });
        }

        public void ChangeRank(IPlayer fromChr, int toId, int toRank)
        {
            _transport.SendChangePlayerGuildRank(new UpdateGuildMemberRankRequest { MasterId = fromChr.Id, TargetPlayerId = toId, NewRank = toRank });
        }

        public void OnChangePlayerGuildRank(UpdateGuildMemberRankResponse data)
        {
            HandleGuildResponse(data.Code, data.GuildId, data.Request.MasterId, guild =>
            {
                return guild.ChangeRank(data.Request.TargetPlayerId, data.Request.NewRank);
            });
        }

        private void HandleGuildResponse(int code, int guildId, int masterId, Func<Guild, bool> onGuildAction, Action<IPlayer>? masterSuccessBack = null, Action<IPlayer>? masterFailback = null)
        {
            if (code == 0)
            {
                var guild = GetGuildById(guildId);
                if (guild != null)
                {
                    var localResult = onGuildAction(guild);

                    if (!localResult)
                    {
                        _logger.LogCritical(
                            "家族数据同步失败：主服务器成功，频道服务器失败，可能存在数据不同步的问题！Server: {ServerName}, GuildId: {GuildId}, StackTrace: {StackTrace}",
                            _serverContainer.ServerName, guildId, new StackTrace());
                        return;
                    }

                    if (masterSuccessBack != null)
                    {
                        var master = _serverContainer.FindPlayerById(masterId);
                        if (master != null)
                        {
                            masterSuccessBack(master);
                        }
                    }
                    return;
                }
            }

            if (masterFailback != null)
            {
                var master = _serverContainer.FindPlayerById(masterId);
                if (master != null)
                {
                    masterFailback(master);
                }
            }
        }


        public void SetGuildEmblem(IPlayer chr, short bg, byte bgcolor, short logo, byte logocolor)
        {
            _transport.SendUpdateGuildEmblem(new Dto.UpdateGuildEmblemRequest
            {
                Logo = logo,
                LogoColor = logocolor,
                LogoBg = bg,
                LogoBgColor = bgcolor
            });
        }

        public void OnGuildEmblemUpdate(UpdateGuildEmblemResponse data)
        {
            HandleGuildResponse(data.Code, data.GuildId, data.Request.MasterId, (guild) =>
            {
                guild.setGuildEmblem((short)data.Request.LogoBg, (byte)data.Request.LogoBgColor, (short)data.Request.Logo, (byte)data.Request.LogoColor);
                GetAllianceById(guild.AllianceId)?.BroadcastGuildAlliance();
                return true;
            }, master =>
            {
                master.gainMeso(-YamlConfig.config.server.CHANGE_EMBLEM_COST, true, false, true);
            });
        }

        public void SetGuildRankTitle(IPlayer chr, string[] titles)
        {
            var request = new Dto.UpdateGuildRankTitleRequest { MasterId = chr.Id };
            request.RankTitles.AddRange(titles);
            _transport.SendUpdateGuildRankTitle(request);
        }
        public void OnGuildRankTitleUpdate(UpdateGuildRankTitleResponse data)
        {
            HandleGuildResponse(data.Code, data.GuildId, data.Request.MasterId, guild =>
            {
                guild.changeRankTitle(data.Request.RankTitles.ToArray());
                return true;
            });
        }


        public void IncreaseGuildCapacity(IPlayer chr, int cost)
        {
            _transport.SendUpdateGuildCapacity(new Dto.UpdateGuildCapacityRequest { MasterId = chr.Id, Cost = cost });
        }

        public void OnGuildCapacityIncreased(UpdateGuildCapacityResponse data)
        {
            HandleGuildResponse(data.Code, data.GuildId, data.Request.MasterId, guild =>
            {
                return guild.increaseCapacity();
            }, master =>
            {
                master.gainMeso(-data.Request.Cost, true, false, true);
            }, master =>
            {
                master.dropMessage(1, "Your guild already reached the maximum capacity of players.");
            });
        }

        public void SetGuildNotice(IPlayer chr, string notice)
        {
            _transport.SendUpdateGuildNotice(new UpdateGuildNoticeRequest { MasterId = chr.Id, Notice = notice });
        }
        public void OnGuildNoticeUpdate(UpdateGuildNoticeResponse data)
        {
            HandleGuildResponse(data.Code, data.GuildId, data.Request.MasterId, guild =>
            {
                guild.setGuildNotice(data.Request.Notice);
                return true;
            });
        }

        public void Disband(IPlayer chr)
        {
            _transport.SendGuildDisband(new Dto.GuildDisbandRequest { MasterId = chr.Id });
        }

        public void OnGuildDisband(Dto.GuildDisbandResponse data)
        {
            HandleGuildResponse(data.Code, data.GuildId, data.Request.MasterId,
                guild =>
                {
                    if (CachedData.TryRemove(guild.GuildId, out _))
                    {
                        guild.disbandGuild();

                        if (guild.AllianceId > 0)
                        {
                            var alliance = GetAllianceById(guild.AllianceId);
                            if (alliance != null)
                            {
                                return alliance.RemoveGuildFromAlliance(guild.GuildId, 1);
                            }
                        }

                        return true;
                    }
                    return false;
                });
        }

        internal void DropGuildMessage(int guildId, int v, string callout)
        {
            _transport.BroadcastGuildMessage(guildId, v, callout);
        }

        public void GainGP(IPlayer chr, int gp)
        {
            _transport.SendUpdateGuildGP(new UpdateGuildGPRequest { MasterId = chr.Id, Gp = gp });
        }

        internal void OnGuildGPUpdate(Dto.UpdateGuildGPResponse data)
        {
            HandleGuildResponse(data.Code, data.GuildId, data.Request.MasterId,
                guild =>
                {
                    if (data.Request.Gp > 0)
                    {
                        guild.gainGP(data.Request.Gp);
                    }
                    else
                    {
                        guild.removeGP(-data.Request.Gp);
                    }
                    return true;
                });
        }

        #region alliance
        public Alliance? CreateAlliance(IPlayer leader, string name)
        {
            var guilds = leader.getPartyMembersOnSameMap().OrderBy(x => x.isLeader()).Select(x => x.Id).ToArray();

            var remoteAlliance = _transport.CreateAlliance(guilds, name);
            if (remoteAlliance.Model == null)
                return null;

            var localData = new Alliance(remoteAlliance.Model.AllianceId);
            var alliance = _mapper.Map(remoteAlliance, localData);
            if (alliance != null)
            {
                var guildObjs = alliance.Guilds;
                alliance.setCapacity(guilds.Length);
                foreach (var guild in guildObjs.Values)
                {
                    CachedData[guild.GuildId] = guild;
                }
                _allianceData[alliance.AllianceId] = alliance;

                alliance.BroadcastAllianceInfo();
                alliance.BroadcastGuildAlliance();
            }

            return alliance;
        }
        public void SendAllianceInvitation(IChannelClient c, string targetGuildName)
        {
            var alliance = c.OnlinedCharacter.AllianceModel;
            if (alliance == null)
                return;

            if (alliance.getGuilds().Count == alliance.getCapacity())
            {
                c.OnlinedCharacter.dropMessage(5, "Your alliance cannot comport any more guilds at the moment.");
            }
            var mg = CachedData.Values.FirstOrDefault(x => x.Name == targetGuildName);
            if (mg == null)
            {
                c.OnlinedCharacter.dropMessage(5, "The entered guild does not exist.");
            }
            else
            {
                if (mg.getAllianceId() > 0)
                {
                    c.OnlinedCharacter.dropMessage(5, "The entered guild is already registered on a guild alliance.");
                }
                else
                {
                    _transport.SendInvitation(new CreateInviteRequest
                    {
                        Type = InviteTypes.Alliance,
                        FromId = c.OnlinedCharacter.Id,
                        ToName = targetGuildName
                    });
                }
            }
        }

        public void AnswerAllianceInvitation(IPlayer chr, int allianceId, bool answer)
        {
            _transport.AnswerInvitation(new AnswerInviteRequest { MasterId = chr.Id, Ok = answer, CheckKey = allianceId, Type = InviteTypes.Alliance });
        }
        public Alliance? GetAllianceById(int id)
        {
            if (_allianceData.TryGetValue(id, out var d) && d != null)
                return d;

            var remoteData = _transport.GetAlliance(id).Model;
            if (remoteData == null)
                return null;

            var localData = new Alliance(id);
            _mapper.Map(remoteData, localData);
            _allianceData[id] = localData;
            return localData;
        }

        public void SendGuildChat(IPlayer chr, string text)
        {
            _transport.SendGuildChat(chr.Name, text);
        }
        public void SendAllianceChat(IPlayer chr, string text)
        {
            _transport.SendAllianceChat(chr.Name, text);
        }


        #endregion

        #region Alliance
        private void HandleAllianceResponse(int code, int allianceId, int masterId, Func<Alliance, bool> onGuildAction, Action<IPlayer, Alliance>? masterSuccessBack = null, Action<IPlayer>? masterFailback = null)
        {
            if (code == 0)
            {
                var alliance = GetAllianceById(allianceId);
                if (alliance != null)
                {
                    var localResult = onGuildAction(alliance);

                    if (!localResult)
                    {
                        _logger.LogCritical(
                            "联盟数据同步失败：主服务器成功，频道服务器失败，可能存在数据不同步的问题！Server: {ServerName}, AllianceId: {AllianceId}, StackTrace: {StackTrace}",
                            _serverContainer.ServerName, allianceId, new StackTrace());
                        return;
                    }

                    if (masterSuccessBack != null)
                    {
                        var master = _serverContainer.FindPlayerById(masterId);
                        if (master != null)
                        {
                            masterSuccessBack(master, alliance);
                        }
                    }
                    return;
                }
            }

            if (masterFailback != null)
            {
                var master = _serverContainer.FindPlayerById(masterId);
                if (master != null)
                {
                    masterFailback(master);
                }
            }
        }
        public void GuildLeaveAlliance(IPlayer player, int guildId)
        {
            if (player.AllianceModel == null || player.GuildRank != 1)
            {
                return;
            }
            _transport.SendGuildLeaveAlliance(new Dto.GuildLeaveAllianceRequest { MasterId = player.Id });
        }

        public void OnGuildLeaveAlliance(GuildLeaveAllianceResponse data)
        {
            HandleAllianceResponse(data.Code, data.AllianceId, data.Request.MasterId,
                alliance =>
                {
                    return alliance.RemoveGuildFromAlliance(data.GuildId, 1);
                });
        }
        public void AllianceExpelGuild(IPlayer player, int allianceId, int guildId)
        {
            if (player.AllianceModel?.AllianceId != allianceId)
            {
                return;
            }

            _transport.SendAllianceExpelGuild(new Dto.AllianceExpelGuildRequest { MasterId = player.Id, GuildId = guildId });
        }
        public void OnAllianceExpelGuild(AllianceExpelGuildResponse data)
        {
            HandleAllianceResponse(data.Code, data.AllianceId, data.Request.MasterId,
                alliance =>
                {
                    return alliance.RemoveGuildFromAlliance(data.GuildId, 2);
                });
        }
        public void OnGuildJoinAlliance(GuildJoinAllianceResponse data)
        {
            HandleAllianceResponse(data.Code, data.AllianceId, data.Request.MasterId,
                alliance =>
                {
                    var guild = GetGuildById(data.GuildId);
                    if (guild == null || !alliance.TryAddGuild(guild))
                    {
                        return false;
                    }
                    alliance.broadcastMessage(GuildPackets.addGuildToAlliance(alliance, guild));
                    alliance.BroadcastAllianceInfo();
                    alliance.BroadcastNotice();
                    guild.dropMessage("Your guild has joined the [" + alliance.getName() + "] union.");

                    return true;
                });
        }
        public void ChageLeaderAllianceRank(IPlayer player, int targetPlayerId)
        {
            if (player.GuildRank != 1)
            {
                return;
            }
            _transport.SendChangeAllianceLeader(new Dto.AllianceChangeLeaderRequest { MasterId = player.Id, PlayerId = targetPlayerId });
        }
        public void OnAllianceLeaderChanged(AllianceChangeLeaderResponse data)
        {
            HandleAllianceResponse(data.Code, data.AllianceId, data.Request.MasterId,
                alliance =>
                {
                    var oldLeader = alliance.GetLeader();
                    oldLeader.AllianceRank = 2;

                    var oldLeaderObj = _serverContainer.FindPlayerById(oldLeader.Channel, oldLeader.Id);
                    if (oldLeaderObj != null)
                        oldLeaderObj.setAllianceRank(2);

                    var newLeader = alliance.GetMemberById(data.Request.PlayerId);
                    newLeader.AllianceRank = 1;
                    var newLeaderObj = _serverContainer.FindPlayerById(newLeader.Channel, newLeader.Id);
                    if (newLeaderObj != null)
                        newLeaderObj.setAllianceRank(1);


                    alliance.BroadcastGuildAlliance();
                    alliance.dropMessage("'" + newLeader.Name + "' has been appointed as the new head of this Alliance.");

                    return true;
                });
        }
        public void ChangePlayerAllianceRank(IPlayer player, int targetPlayerId, bool isIncrease)
        {
            _transport.SendChangePlayerAllianceRank(new Dto.ChangePlayerAllianceRankRequest { MasterId = player.Id, PlayerId = targetPlayerId, Delta = isIncrease ? 1 : -1 });
        }
        public void OnPlayerAllianceRankChanged(ChangePlayerAllianceRankResponse data)
        {
            HandleAllianceResponse(data.Code, data.AllianceId, data.Request.MasterId,
                alliance =>
                {
                    var newRank = data.NewRank;
                    var targetPlayer = alliance.GetMemberById(data.Request.PlayerId);
                    targetPlayer.AllianceRank = newRank;
                    var targetPlayerObj = _serverContainer.FindPlayerById(targetPlayer.Channel, targetPlayer.Id);
                    if (targetPlayerObj != null)
                        targetPlayerObj.setAllianceRank(newRank);

                    alliance.BroadcastGuildAlliance();

                    return true;
                });
        }
        public void HandleIncreaseAllianceCapacity(IPlayer chr)
        {
            _transport.SendIncreaseAllianceCapacity(new Dto.IncreaseAllianceCapacityRequest { MasterId = chr.Id });
        }
        public void OnAllianceCapacityIncreased(IncreaseAllianceCapacityResponse data)
        {
            HandleAllianceResponse(data.Code, data.AllianceId, data.Request.MasterId,
                alliance =>
                {
                    alliance.increaseCapacity(1);
                    alliance.BroadcastGuildAlliance();
                    alliance.BroadcastNotice();

                    return true;
                },
                (mc, alliance) =>
                {
                    mc.sendPacket(GuildPackets.updateAllianceInfo(alliance));  // thanks Vcoc for finding an alliance update to leader issue
                });
        }
        internal void UpdateAllianceRank(IPlayer chr, string[] ranks)
        {
            var request = new Dto.UpdateAllianceRankTitleRequest() { MasterId = chr.Id };
            request.RankTitles.AddRange(ranks);
            _transport.SendUpdateAllianceRankTitle(request);
        }
        public void OnAllianceRankTitleChanged(UpdateAllianceRankTitleResponse data)
        {
            HandleAllianceResponse(data.Code, data.AllianceId, data.Request.MasterId,
                alliance =>
                {
                    var newRanks = data.Request.RankTitles.ToArray();
                    alliance.setRankTitle(newRanks);
                    alliance.broadcastMessage(GuildPackets.changeAllianceRankTitle(alliance.getId(), newRanks), -1, -1);

                    return true;
                });
        }
        internal void UpdateAllianceNotice(IPlayer chr, string notice)
        {
            _transport.SendUpdateAllianceNotice(new Dto.UpdateAllianceNoticeRequest { MasterId = chr.Id, Notice = notice });
        }
        public void OnAllianceNoticeChanged(UpdateAllianceNoticeResponse data)
        {
            HandleAllianceResponse(data.Code, data.AllianceId, data.Request.MasterId,
                alliance =>
                {
                    alliance.setNotice(data.Request.Notice);
                    alliance.BroadcastNotice();

                    alliance.dropMessage(5, "* Alliance Notice : " + data.Request.Notice);

                    return true;
                });
        }

        internal void DisbandAlliance(IPlayer player, int allianceId)
        {
            _transport.SendAllianceDisband(new Dto.DisbandAllianceRequest { MasterId = player.Id });
        }
        public void OnAllianceDisband(DisbandAllianceResponse data)
        {
            HandleAllianceResponse(data.Code, data.AllianceId, data.Request.MasterId,
                alliance =>
                {
                    alliance.Disband();
                    return _allianceData.TryRemove(alliance.AllianceId, out _);
                });
        }

        internal void ShowRankedGuilds(IChannelClient c, int npc)
        {
            var data = _transport.RequestRankedGuilds();
            c.sendPacket(GuildPackets.showGuildRanks(npc, data.Guilds.ToList()));
        }
        #endregion
    }
}
