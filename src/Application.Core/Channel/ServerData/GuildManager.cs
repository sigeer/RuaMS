using Application.Core.Game.Invites;
using Application.Core.Game.Players;
using Application.Core.Game.Relation;
using Application.Core.Managers;
using Application.Core.ServerTransports;
using Application.Shared.Guild;
using Application.Shared.Team;
using AutoMapper;
using AutoMapper.Execution;
using constants.game;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI.Common;
using net.server.coordinator.matchchecker;
using net.server.guild;
using Org.BouncyCastle.Asn1.X509;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Xml.Linq;
using tools;
using static Mysqlx.Notice.Warning.Types;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public GuildResponse? SendInvitation(IChannelClient c, string targetName)
        {
            var sender = c.OnlinedCharacter;

            var mc = c.CurrentServer.getPlayerStorage().getCharacterByName(targetName);
            if (mc == null)
            {
                return GuildResponse.NOT_IN_CHANNEL;
            }
            if (mc.GuildId > 0)
            {
                return GuildResponse.ALREADY_IN_GUILD;
            }

            if (InviteType.GUILD.CreateInvite(new GuildInviteRequest(sender, mc)))
            {
                mc.sendPacket(GuildPackets.guildInvite(sender.GuildId, sender.getName()));
                return null;
            }
            else
            {
                return GuildResponse.MANAGING_INVITE;
            }
        }

        public bool AnswerInvitation(IPlayer answer, int guildId, bool operation)
        {
            InviteResult res = InviteType.GUILD.AnswerInvite(answer.Id, guildId, operation);

            GuildResponse? mgr = null;
            switch (res.Result)
            {
                case InviteResultType.ACCEPTED:
                    return true;

                case InviteResultType.DENIED:
                    mgr = GuildResponse.DENIED_INVITE;
                    break;

                default:
                    mgr = GuildResponse.NOT_FOUND_INVITE;
                    break;
            }

            if (mgr != null && res.Request != null)
            {
                res.Request.From.sendPacket(mgr.Value.getPacket(answer.getName()));
            }
            return false;
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

                chr.saveGuildStatus(); // update database
            }

            guild.BroadcastDisplay();


            return guild;
        }

        public bool AddMember(IPlayer player, int guildId)
        {
            return UpdateGuildMember(GuildOperation.AddMember, player, player.Id, guildId);
        }


        public bool LeaveMember(IPlayer fromChr)
        {
            return UpdateGuildMember(GuildOperation.Leave, fromChr, fromChr.Id, fromChr.GuildId);
        }

        public Guild? GetGuildById(int id)
        {
            if (CachedData.TryGetValue(id, out var d) && d != null)
                return d;

            var localData = new Guild(_serverContainer, id);
            _mapper.Map(_transport.GetGuild(id), localData);
            CachedData[localData.GuildId] = localData;
            return localData;
        }

        public bool ProcessUpdateGuild(Dto.UpdateGuildResponse result)
        {
            if (result.UpdateType == 1)
                return ProcessUpdateMember((GuildOperation)result.Operation, result.GuildId, _mapper.Map<GuildMember>(result.UpdatedMember));
            if (result.UpdateType == 2)
                return ProcessUpdateGuildMeta((GuildInfoOperation)result.Operation, result.GuildId, result.UpdatedGuild);
            return false;
        }
        public bool ProcessUpdateMember(GuildOperation operation, int guildId, GuildMember targetMember)
        {
            var guild = GetGuildById(guildId);
            if (guild == null)
                return false;
            bool result = false;

            switch (operation)
            {
                case GuildOperation.AddMember:
                    result = guild.AddGuildMember(targetMember);
                    break;
                case GuildOperation.ChangeRank:
                    result = guild.ChangeRank(targetMember.Id, targetMember.GuildRank);
                    break;
                case GuildOperation.ExpelMember:
                    result = guild.ExpelMember(targetMember.Id);
                    break;
                case GuildOperation.Leave:
                    result = guild.LeaveGuild(targetMember.Id);
                    break;
                case GuildOperation.MemberJobChanged:
                    guild.broadcast(PacketCreator.jobMessage(0, targetMember.JobId, targetMember.Name), targetMember.Id);
                    guild.broadcast(GuildPackets.guildMemberLevelJobUpdate(guild, targetMember));
                    break;
                case GuildOperation.MemberLevelChanged:
                    guild.broadcast(PacketCreator.levelUpMessage(0, targetMember.Level, targetMember.Name), targetMember.Id);
                    guild.broadcast(GuildPackets.guildMemberLevelJobUpdate(guild, targetMember));
                    break;
                case GuildOperation.MemberLogin:
                case GuildOperation.MemberLogoff:
                    guild.setOnline(targetMember.Id, operation == GuildOperation.MemberLogin, targetMember.Channel);
                    break;
                default:
                    break;
            }

            _logger.LogCritical(
                "家族数据同步失败：主服务器成功，频道服务器失败，可能存在数据不同步的问题！Server: {ServerInstance}, Operation: {Operation}, GuildId: {GuildId}, TargetMemberId: {TargetMemberId}",
                _serverContainer.ServerName, operation, guildId, targetMember.Id);
            return result;
        }

        public bool ProcessUpdateGuildMeta(GuildInfoOperation operation, int guildId, Dto.GuildDto targetGuild)
        {
            var guild = GetGuildById(guildId);
            if (guild == null)
                return false;

            switch (operation)
            {
                case GuildInfoOperation.ChangeName:
                    guild.Name = targetGuild.Name;
                    guild.broadcastNameChanged();
                    break;
                case GuildInfoOperation.ChangeEmblem:
                    guild.setGuildEmblem((short)targetGuild.LogoBg, (byte)targetGuild.LogoBgColor, (short)targetGuild.Logo, (byte)targetGuild.LogoColor);
                    guild.BroadcastDisplay();
                    break;
                case GuildInfoOperation.ChangeRankTitle:
                    guild.changeRankTitle([targetGuild.Rank1Title, targetGuild.Rank2Title, targetGuild.Rank3Title, targetGuild.Rank4Title, targetGuild.Rank5Title]);
                    break;
                case GuildInfoOperation.ChangeNotice:
                    guild.setGuildNotice(targetGuild.Notice);
                    break;
                case GuildInfoOperation.IncreaseCapacity:
                    guild.increaseCapacity();
                    break;
                case GuildInfoOperation.Disband:
                    guild.disbandGuild();
                    break;
                default:
                    break;
            }
            return true;
        }

        public bool ExpelMember(IPlayer fromChr, int toId)
        {
            return UpdateGuildMember(GuildOperation.ExpelMember, fromChr, toId, fromChr.GuildId);
        }

        public bool ChangeRank(IPlayer fromChr, int toId, int toRank)
        {
            return UpdateGuildMember(GuildOperation.ChangeRank, fromChr, toId, fromChr.GuildId);
        }

        public bool UpdateGuildMember(GuildOperation operation, IPlayer player, int target, int guildId, int toRank = -1)
        {
            var result = _transport.SendUpdateGuildMember(_serverContainer.ServerName, operation, player.Id, guildId, target, toRank);
            if (result.Code == 0)
            {
                return ProcessUpdateGuild(result);
            }

            return false;
        }


        public bool UpdateGuildMeta(GuildInfoOperation operation, IPlayer player, int guildId, Dto.GuildDto updateFields)
        {
            var result = _transport.SendUpdateGuildMeta(_serverContainer.ServerName, operation, player.Id, guildId, updateFields);
            if (result.Code == 0)
            {
                return ProcessUpdateGuild(result);
            }

            return false;
        }

        public bool SetGuildEmblem(IPlayer chr, short bg, byte bgcolor, short logo, byte logocolor)
        {
            return UpdateGuildMeta(GuildInfoOperation.ChangeEmblem, chr, chr.GuildId, new Dto.GuildDto
            {
                Logo = logo,
                LogoColor = logocolor,
                LogoBg = bg,
                LogoBgColor = bgcolor
            });
        }

        public bool SetGuildRankTitle(IPlayer chr, string[] titles)
        {
            return UpdateGuildMeta(GuildInfoOperation.ChangeRankTitle, chr, chr.GuildId, new Dto.GuildDto
            {
                Rank1Title = titles[0],
                Rank2Title = titles[1],
                Rank3Title = titles[2],
                Rank4Title = titles[3],
                Rank5Title = titles[4]
            });
        }

        public bool IncreaseGuildCapacity(IPlayer chr)
        {
            return UpdateGuildMeta(GuildInfoOperation.IncreaseCapacity, chr, chr.GuildId, new Dto.GuildDto
            {
            });
        }

        public bool SetGuildNotice(IPlayer chr, string? notice)
        {
            return UpdateGuildMeta(GuildInfoOperation.ChangeRankTitle, chr, chr.GuildId, new Dto.GuildDto
            {
                Notice = notice
            });
        }

        public bool Disband(IPlayer chr)
        {
            return UpdateGuildMeta(GuildInfoOperation.Disband, chr, chr.GuildId, new Dto.GuildDto
            {
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
        public void SendAllianceInvitation(IChannelClient c, string targetGuildName, int allianceId)
        {
            var mg = AllGuildStorage.GetGuildByName(targetGuildName);
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
                    var victim = c.CurrentServer.Players.getCharacterById(mg.getLeaderId());
                    if (victim == null)
                    {
                        c.OnlinedCharacter.dropMessage(5, "The master of the guild that you offered an invitation is currently not online.");
                    }
                    else
                    {
                        if (InviteType.ALLIANCE.CreateInvite(new AllianceInviteRequest(c.OnlinedCharacter, victim)))
                        {
                            victim.sendPacket(GuildPackets.allianceInvite(allianceId, c.OnlinedCharacter));
                        }
                        else
                        {
                            c.OnlinedCharacter.dropMessage(5, "The master of the guild that you offered an invitation is currently managing another invite.");
                        }
                    }
                }
            }
        }

        public bool AnswerAllianceInvitation(int targetId, string targetGuildName, int allianceId, bool answer)
        {
            InviteResult res = InviteType.ALLIANCE.AnswerInvite(targetId, allianceId, answer);

            string msg;
            switch (res.Result)
            {
                case InviteResultType.ACCEPTED:
                    return true;

                case InviteResultType.DENIED:
                    msg = "[" + targetGuildName + "] guild has denied your guild alliance invitation.";
                    break;

                default:
                    msg = "The guild alliance request has not been accepted, since the invitation expired.";
                    break;
            }

            if (res.Request != null)
            {
                res.Request.From.dropMessage(5, msg);
            }

            return false;
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
        public bool ProcessAllianceUpdate(Dto.UpdateAllianceResponse updates)
        {
            var alliance = GetAllianceById(updates.AllianceId);
            if (alliance == null)
                return false;

            var guild = GetGuildById(updates.GuildId);
            if (guild == null)
                return false;

            var operation = (AllianceOperation)updates.Operation;
            switch (operation)
            {
                case AllianceOperation.LeaveAlliance:
                    alliance.RemoveGuildFromAlliance(updates.GuildId, 1);
                    break;
                case AllianceOperation.ExpelGuild:
                    alliance.RemoveGuildFromAlliance(updates.GuildId, 2);
                    break;
                case AllianceOperation.Join:
                    if (!alliance.TryAddGuild(guild))
                    {
                        return false;
                    }
                    alliance.broadcastMessage(GuildPackets.addGuildToAlliance(alliance, guild), -1, -1);
                    alliance.BroadcastAllianceInfo();
                    alliance.BroadcastNotice();
                    guild.dropMessage("Your guild has joined the [" + alliance.getName() + "] union.");
                    break;
                case AllianceOperation.MemberLogin:
                    alliance.UpdateMember(_mapper.Map<GuildMember>(updates.TargetPlayer));
                    alliance.broadcastMessage(GuildPackets.allianceMemberOnline(guild, updates.TargetPlayer.Id, true), updates.TargetPlayer.Id, -1);
                    break;
                case AllianceOperation.MemberUpdate:
                    alliance.UpdateMember(_mapper.Map<GuildMember>(updates.TargetPlayer));
                    alliance.broadcastMessage(GuildPackets.updateAllianceJobLevel(guild, _mapper.Map<GuildMember>(updates.TargetPlayer)), updates.TargetPlayer.Id, -1);
                    break;

                case AllianceOperation.ChangeAllianceLeader:
                    var oldLeader = alliance.GetLeader();
                    oldLeader.AllianceRank = 2;

                    var oldLeaderObj = _serverContainer.FindPlayerById(oldLeader.Channel, oldLeader.Id);
                    if (oldLeaderObj != null)
                        oldLeaderObj.setAllianceRank(2);

                    var newLeader = alliance.GetMemberById(updates.TargetPlayer.Id);
                    newLeader.AllianceRank = 1;
                    var newLeaderObj = _serverContainer.FindPlayerById(updates.TargetPlayer.Channel, updates.TargetPlayer.Id);
                    if (newLeaderObj != null)
                        newLeaderObj.setAllianceRank(1);


                    alliance.BroadcastGuildAlliance();
                    alliance.dropMessage("'" + updates.TargetPlayer.Name + "' has been appointed as the new head of this Alliance.");
                    break;
                case AllianceOperation.IncreasePlayerRank:
                case AllianceOperation.DecreasePlayerRank:
                    var newRank = updates.TargetPlayer.AllianceRank;
                    var targetPlayer = alliance.GetMemberById(updates.TargetPlayer.Id);
                    targetPlayer.AllianceRank = newRank;
                    var targetPlayerObj = _serverContainer.FindPlayerById(updates.TargetPlayer.Channel, updates.TargetPlayer.Id);
                    if (targetPlayerObj != null)
                        targetPlayerObj.setAllianceRank(newRank);

                    alliance.BroadcastGuildAlliance();
                    break;
                case AllianceOperation.IncreaseCapacity:
                    alliance.increaseCapacity(1);
                    alliance.BroadcastGuildAlliance();
                    alliance.BroadcastNotice();
                    break;
                case AllianceOperation.Disband:
                    alliance.Disband();
                    _allianceData.TryRemove(updates.AllianceId, out _);
                    break;
                case AllianceOperation.ChangeRankTitle:
                    var newRanks = updates.UpdatedAlliance.RankTitles.ToArray();
                    alliance.setRankTitle(newRanks);
                    alliance.broadcastMessage(GuildPackets.changeAllianceRankTitle(alliance.getId(), newRanks), -1, -1);
                    break;
                case AllianceOperation.ChangeNotice:
                    alliance.setNotice(updates.UpdatedAlliance.Notice);
                    alliance.BroadcastNotice();

                    alliance.dropMessage(5, "* Alliance Notice : " + updates.UpdatedAlliance.Notice);
                    break;
                default:
                    break;
            }
            return true;
        }

        private bool UpdateAlliance(Dto.UpdateAllianceRequest request)
        {
            var result = _transport.SendUpdateAlliance(request);
            if (result.Code == 0)
                return ProcessAllianceUpdate(result);

            return false;
        }

        public bool ChageLeaderAllianceRank(IPlayer player, int targetPlayerId)
        {
            if (player.AllianceModel == null || player.GuildRank != 1)
            {
                return false;
            }
            return UpdateAlliance(new Dto.UpdateAllianceRequest
            {
                Operation = (int)AllianceOperation.ChangeAllianceLeader,
                AllianceId = player.AllianceModel.AllianceId,
                FromChannel = _serverContainer.ServerName,
                OperatorPlayerId = player.Id,
                TargetPlayerId = targetPlayerId
            });
        }
        public bool ChangePlayerAllianceRank(IPlayer player, int targetPlayerId, bool isIncrease)
        {
            if (player.AllianceModel == null)
            {
                return false;
            }
            return UpdateAlliance(new Dto.UpdateAllianceRequest
            {
                Operation = (int)(isIncrease ? AllianceOperation.IncreasePlayerRank : AllianceOperation.DecreasePlayerRank),
                AllianceId = player.AllianceModel.AllianceId,
                FromChannel = _serverContainer.ServerName,
                OperatorPlayerId = player.Id,
                TargetPlayerId = targetPlayerId
            });
        }

        public bool AllianceLeaveGuild(IPlayer player, int guildId)
        {
            if (player.AllianceModel == null || player.GuildRank != 1)
            {
                return false;
            }
            return UpdateAlliance(new Dto.UpdateAllianceRequest
            {
                Operation = (int)AllianceOperation.LeaveAlliance,
                AllianceId = player.AllianceModel.AllianceId,
                FromChannel = _serverContainer.ServerName,
                OperatorPlayerId = player.Id,
                TargetGuildId = guildId,
            });
        }

        public bool AllianceExpelGuild(IPlayer player, int allianceId, int guildId)
        {
            if (player.AllianceModel?.AllianceId != allianceId)
            {
                return false;
            }
            return UpdateAlliance(new Dto.UpdateAllianceRequest
            {
                Operation = (int)AllianceOperation.ExpelGuild,
                AllianceId = player.AllianceModel.AllianceId,
                FromChannel = _serverContainer.ServerName,
                OperatorPlayerId = player.Id,
                TargetGuildId = guildId,
            });
        }

        public bool GuildJoinAlliance(IPlayer player, int allianceId, int guildId)
        {
            var alliance = GetAllianceById(allianceId);
            if (alliance == null)
            {
                return false;
            }

            if (!AnswerAllianceInvitation(player.getId(), player.getGuild()!.Name, allianceId, true))
            {
                return false;
            }

            if (alliance.getGuilds().Count == alliance.getCapacity())
            {
                player.dropMessage(5, "Your alliance cannot comport any more guilds at the moment.");
                return false;
            }

            return UpdateAlliance(new Dto.UpdateAllianceRequest
            {
                Operation = (int)AllianceOperation.Join,
                AllianceId = allianceId,
                FromChannel = _serverContainer.ServerName,
                OperatorPlayerId = player.Id,
                TargetGuildId = guildId,
            });
        }

        internal bool DisbandAlliance(IPlayer player, int allianceId)
        {
            return UpdateAlliance(new Dto.UpdateAllianceRequest
            {
                Operation = (int)AllianceOperation.Disband,
                AllianceId = allianceId,
                FromChannel = _serverContainer.ServerName,
                OperatorPlayerId = player.Id,
            });
        }

        internal bool UpdateAllianceRank(IPlayer chr, string[] ranks)
        {
            var request = new Dto.AllianceDto();
            request.RankTitles.AddRange(ranks);
            return UpdateAlliance(new Dto.UpdateAllianceRequest
            {
                Operation = (int)AllianceOperation.Disband,
                AllianceId = chr.AllianceModel.AllianceId,
                FromChannel = _serverContainer.ServerName,
                OperatorPlayerId = chr.Id,
                UpdateFields = request,
            });
        }

        internal bool UpdateAllianceNotice(IPlayer chr, string notice)
        {
            var request = new Dto.AllianceDto() { Notice = notice };
            return UpdateAlliance(new Dto.UpdateAllianceRequest
            {
                Operation = (int)AllianceOperation.Disband,
                AllianceId = chr.AllianceModel.AllianceId,
                FromChannel = _serverContainer.ServerName,
                OperatorPlayerId = chr.Id,
                UpdateFields = request,
            });
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
    }
}
