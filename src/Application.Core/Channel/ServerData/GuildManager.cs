using Application.Core.Game.Invites;
using Application.Core.Game.Players;
using Application.Core.Game.Relation;
using Application.Core.Managers;
using Application.Core.ServerTransports;
using Application.Shared.Guild;
using Application.Shared.Team;
using AutoMapper;
using AutoMapper.Execution;
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
using System.Security.Cryptography;
using System.Xml.Linq;
using tools;
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

        public Guild? CreateGuild( string guildName, int playerId)
        {
            var remoteData = _transport.CreateGuild(guildName, playerId).Model;
            if (remoteData == null)
                return null;

            var localData = new Guild(_serverContainer, remoteData.GuildId);
            _mapper.Map(remoteData, localData);
            CachedData[localData.GuildId] = localData;
            return localData;
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

        public bool ProcessUpdateGuild(WorldChannel worldChannel, Dto.UpdateGuildResponse result)
        {
            if (result.UpdateType == 1)
                return ProcessUpdateMember(worldChannel, (GuildOperation)result.Operation, result.GuildId, _mapper.Map<GuildMember>(result.UpdatedMember));
            if (result.UpdateType == 2)
                return ProcessUpdateGuildMeta(worldChannel, (GuildInfoOperation)result.Operation, result.GuildId, result.UpdatedGuild);
            return false;
        }
        public bool ProcessUpdateMember(WorldChannel worldChannel, GuildOperation operation, int guildId, GuildMember targetMember)
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
                default:
                    break;
            }

            _logger.LogCritical(
                "家族数据同步失败：主服务器成功，频道服务器失败，可能存在数据不同步的问题！Server: {ServerInstance}, Operation: {Operation}, GuildId: {GuildId}, TargetMemberId: {TargetMemberId}",
                worldChannel.InstanceId, operation, guildId, targetMember.Id);
            return result;
        }

        public bool ProcessUpdateGuildMeta(WorldChannel worldChannel, GuildInfoOperation operation, int guildId, Dto.GuildDto targetGuild)
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
            var result = _transport.SendUpdateGuildMember(player.getChannelServer().getId(), operation, player.Id, guildId, target, toRank);
            if (result.Code == 0)
            {
                return ProcessUpdateGuild(player.getChannelServer(), result);
            }

            return false;
        }


        public bool UpdateGuildMeta(GuildInfoOperation operation, IPlayer player, int guildId, Dto.GuildDto updateFields)
        {
            var result = _transport.SendUpdateGuildMeta(player.getChannelServer().getId(), operation, player.Id, guildId, updateFields);
            if (result.Code == 0)
            {
                return ProcessUpdateGuild(player.getChannelServer(), result);
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
            var guildMasters = leader.getPartyMembersOnSameMap();
            if (guildMasters.Count != 2)
            {
                return null;
            }

            List<int> guilds = new();
            foreach (var mc in guildMasters)
            {
                guilds.Add(mc.getGuildId());
            }
            var alliance = AllianceManager.createAllianceOnDb(guilds, name);
            if (alliance != null)
            {
                alliance.setCapacity(guilds.Count);
                foreach (int g in guilds)
                {
                    alliance.AddGuild(g);
                }

                int id = alliance.getId();
                try
                {
                    for (int i = 0; i < guildMasters.Count; i++)
                    {
                        var guild = GetGuildById(guilds[i]);
                        if (guild != null)
                        {
                            guild.setAllianceId(id);
                            guild.resetAllianceGuildPlayersRank();
                        }

                        var chr = guildMasters[i];
                        chr.AllianceRank = (i == 0) ? 1 : 2;
                        chr.saveGuildStatus();
                    }

                    AllAllianceStorage.AddOrUpdate(alliance);

                    int worldid = guildMasters.get(0).getWorld();
                    alliance.broadcastMessage(GuildPackets.updateAllianceInfo(alliance), -1, -1);
                    alliance.broadcastMessage(GuildPackets.getGuildAlliances(alliance), -1, -1);  // thanks Vcoc for noticing guilds from other alliances being visually stacked here due to this not being updated
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                    return null;
                }
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
        public Alliance? GetAllianceById(int id) => _allianceData.GetValueOrDefault(id);
        public bool ProcessAllianceMemberUpdate(AllianceOperation operation, int allianceId, int guildId)
        {
            var alliance = GetAllianceById(allianceId);
            if (alliance == null)
                return false;


            switch (operation)
            {
                case AllianceOperation.LeaveAlliance:
                    alliance.RemoveGuildFromAlliance(guildId, 1);
                    break;
                case AllianceOperation.ExpelGuild:
                    alliance.RemoveGuildFromAlliance(guildId, 2);
                    break;
                case AllianceOperation.AddGuild:
                    break;
                default:
                    break;
            }
            return true;
        }

        public bool ProcessAllianceMetaUpdate(AllianceMetaOperation operation, int allianceId, int guildId)
        {
            var alliance = GetAllianceById(allianceId);
            if (alliance == null)
                return false;


            switch (operation)
            {
                case AllianceMetaOperation.IncreaseCapacity:
                    alliance.increaseCapacity(1);
                    break;
                case AllianceMetaOperation.Disband:
                    alliance.Disband();
                    _allianceData.TryRemove(allianceId, out _);
                    break;
                default:
                    break;
            }
            return true;
        }
        public bool ProcessAllianceUpdate(WorldChannel worldChannel, Dto.UpdateAllianceResponse result)
        {
            if (result.UpdateType == 1)
                return ProcessAllianceMemberUpdate((AllianceOperation)result.Operation, result.AllianceId, result.GuildId);
            if (result.UpdateType == 2)
                return ProcessAllianceMetaUpdate((AllianceMetaOperation)result.Operation, result.AllianceId, result.GuildId);
            return false;
        }
        private bool UpdateAllianceMember(AllianceOperation operation, IPlayer player, int allianceId, int guildId)
        {
            var result = _transport.SendUpdateAllianceMember(operation, player.Id, allianceId, guildId);
            if (result.Code == 0)
                return ProcessAllianceUpdate(result);

            return false;
        }

        private bool UpdateAllianceMeta(AllianceMetaOperation operation, IPlayer player, int allianceId, Dto.AllianceDto updates)
        {
            var result = _transport.SendUpdateAllianceMeta(operation, player.Id, allianceId, updates);
            if (result.Code == 0)
                return ProcessAllianceUpdate(result);

            return false;
        }

        public bool AllianceLeaveGuild(IPlayer player, int guildId)
        {
            return UpdateAllianceMember(AllianceOperation.LeaveAlliance, player, 0, guildId);
        }

        public bool AllianceExpelGuild(IPlayer player, int allianceId, int guildId)
        {
            return UpdateAllianceMember(AllianceOperation.ExpelGuild, player, allianceId, guildId);
        }

        public bool GuildJoinAlliance(IPlayer player, int allianceId, int guildId)
        {
            return UpdateAllianceMember(AllianceOperation.AddGuild, player, allianceId, guildId);
        }

        internal bool DisbandAlliance(IPlayer player, int allianceId)
        {
            return UpdateAllianceMeta(AllianceMetaOperation.Disband, player, allianceId, new Dto.AllianceDto());
        }
        #endregion
    }
}
