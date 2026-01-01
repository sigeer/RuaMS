using Application.Core.Channel;
using Application.Shared.Guild;
using Application.Shared.Net;
using AutoMapper.Execution;
using Microsoft.AspNetCore.Hosting.Server;
using net.server.guild;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using tools;
using XmlWzReader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Core.Game.Relation;


public class Guild
{
    public int GuildId { get; }

    public int Leader { get; set; }

    public int GP { get; set; }

    public int Logo { get; set; }

    public short LogoColor { get; set; }

    public string Name { get; set; } = null!;

    public int Capacity { get; set; }

    public int LogoBg { get; set; }

    public short LogoBgColor { get; set; }

    public string? Notice { get; set; }

    public long Signature { get; set; }

    public int AllianceId { get; set; }
    // 1 = master, 2 = jr, 5 = lowest member
    public string[] RankTitles { get; set; } = new string[5];

    public ConcurrentDictionary<int, GuildMember> Members { get; init; }
    private object membersLock = new object();

    private bool channelDirty;

    WorldChannelServer _serverContainer;
    public Guild(WorldChannelServer serverContainer, int guildId)
    {
        _serverContainer = serverContainer;
        channelDirty = new();
        GuildId = guildId;
        this.Members = [];
    }

    /// <summary>
    /// 相当于原先的 notifications
    /// </summary>
    List<IPlayer?>? channelMembersCache;
    List<IPlayer?> GetCurrentChannelMembers()
    {
        if (!channelDirty && channelMembersCache != null)
        {
            return channelMembersCache;
        }

        // 人员发生变动、或者上下线时重算
        channelDirty = true;
        return channelMembersCache = Members.Keys.Select(x => _serverContainer.FindPlayerById(x)).ToList();
    }

    public void HandleCurrentServerMembers(Action<IPlayer> action)
    {
        foreach (var m in GetCurrentChannelMembers())
        {
            if (m != null && m.isLoggedinWorld())
            {
                action(m);
            }
        }
    }

    public void OnMemberChannelChanged(int cid, int channel)
    {
        if (Members.TryGetValue(cid, out var member))
        {
            member.Channel = channel;

            broadcast(GuildPackets.guildMemberOnline(GuildId, cid, channel > 0), cid);
            channelDirty = true;

            if (AllianceId > 0)
            {
                var alliance = _serverContainer.GuildManager.GetAllianceById(AllianceId);
                if (alliance != null)
                {
                    alliance.broadcastMessage(GuildPackets.allianceMemberOnline(this, cid, channel > 0), cid);
                }
            }
        }
    }
    public void OnMemberLevelChanged(int cid, int level)
    {
        if (Members.TryGetValue(cid, out var member))
        {
            member.Level = level;

            broadcast(PacketCreator.levelUpMessage(2, level, member.Name), member.Id);
            broadcast(GuildPackets.guildMemberLevelJobUpdate(GuildId, member.Id, member.Level, member.JobId));

            if (AllianceId > 0)
            {
                var alliance = _serverContainer.GuildManager.GetAllianceById(AllianceId);
                if (alliance != null)
                {
                    alliance.broadcastMessage(GuildPackets.updateAllianceJobLevel(this, member.Id, member.Level, member.JobId), member.Id, -1);
                }
            }
        }
    }

    public void OnMemberJobChanged(int cid, int jobId)
    {
        if (Members.TryGetValue(cid, out var member))
        {
            member.JobId = jobId;

            broadcast(PacketCreator.jobMessage(0, jobId, member.Name), member.Id);
            broadcast(GuildPackets.guildMemberLevelJobUpdate(GuildId, member.Id, member.Level, member.JobId));

            if (AllianceId > 0)
            {
                var alliance = _serverContainer.GuildManager.GetAllianceById(AllianceId);
                if (alliance != null)
                {
                    alliance.broadcastMessage(GuildPackets.updateAllianceJobLevel(this, member.Id, member.Level, member.JobId), member.Id, -1);
                }
            }
        }
    }

    public void UpdateMember(GuildMember member)
    {
        if (member.GuildId != GuildId)
        {
            return;
        }
        Members[member.Id] = member;
        var chr = _serverContainer.FindPlayerById(member.Id);
        if (chr != null)
        {
            chr.GuildRank = member.GuildRank;
            chr.AllianceRank = member.AllianceRank;
        }
    }

    public List<GuildMember> getMembers()
    {
        Monitor.Enter(membersLock);
        try
        {
            return new(Members.Values);
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }



    public string getRankTitle(int rank)
    {
        return RankTitles[rank - 1];
    }

    public bool AddGuildMember(GuildMember member)
    {
        Monitor.Enter(membersLock);
        try
        {
            if (Members.Count >= Capacity)
            {
                return false;
            }

            if (Members.TryAdd(member.Id, member))
            {
                HandleCurrentServerMembers(m =>
                {
                    if (m.Id == member.Id)
                    {
                        SetOnlinedPlayerGuildInfo(m, GuildId, 5);
                    }
                    m.sendPacket(GuildPackets.newGuildMember(GuildId, member));
                });
                channelDirty = true;

                if (AllianceId > 0)
                {
                    _serverContainer.GuildManager.GetAllianceById(AllianceId)?.updateAlliancePackets();
                }
                return true;
            }

            return false;
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public bool OnMemberLeftGuild(int cid)
    {
        if (Members.TryRemove(cid, out var member))
        {
            HandleCurrentServerMembers(m =>
            {
                if (m.Id == cid)
                {
                    m.sendPacket(GuildPackets.updateGP(m.GuildId, 0));
                    m.sendPacket(GuildPackets.showGuildInfo(null));

                    SetOnlinedPlayerGuildInfo(m, 0, 5);
                }
                m.sendPacket(GuildPackets.memberLeft(m, false));
            });
            channelDirty = true;

            if (AllianceId > 0)
            {
                _serverContainer.GuildManager.GetAllianceById(AllianceId)?.updateAlliancePackets();
            }
            return true;
        }
        return false;
    }

    public bool OnMemberExpelled(int cid)
    {
        if (Members.TryRemove(cid, out var member))
        {
            HandleCurrentServerMembers(m =>
            {
                if (m.Id == cid)
                {
                    SetOnlinedPlayerGuildInfo(m, 0, 5);
                }
                m.sendPacket(GuildPackets.memberLeft(m, true));
            });

            if (AllianceId > 0)
            {
                _serverContainer.GuildManager.GetAllianceById(AllianceId)?.updateAlliancePackets();
            }

            channelDirty = true;
            return true;
        }
        return false;
    }
    internal bool OnMemberRankChanged(int cid, int newRank)
    {
        Monitor.Enter(membersLock);
        try
        {
            if (Members.TryGetValue(cid, out var member))
            {
                member.GuildRank = newRank;

                HandleCurrentServerMembers(m =>
                {
                    if (m.Id == cid)
                    {
                        SetOnlinedPlayerGuildInfo(m, GuildId, newRank);
                    }
                    m.sendPacket(GuildPackets.changeRank(GuildId, m.Id, m.Rank));
                });
                return true;
            }
            return false;
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public void OnNoticeChanged(string notice)
    {
        this.Notice = notice;

        this.broadcast(GuildPackets.guildNotice(GuildId, notice));
    }

    public override bool Equals(object? other)
    {
        if (other is Guild o)
        {
            return (o.GuildId == GuildId);
        }
        return false;
    }

    public override int GetHashCode()
    {
        int hash = 3;
        hash = 89 * hash + (this.Name != null ? this.Name.GetHashCode() : 0);
        hash = 89 * hash + this.GuildId;
        return hash;
    }

    public void OnRankTitleChanged(string[] ranks)
    {
        Array.ConstrainedCopy(ranks, 0, RankTitles, 0, 5);

        this.broadcast(GuildPackets.rankTitleChange(GuildId, ranks));
    }

    public void OnGuildDisband()
    {
        Members.Clear();

        HandleCurrentServerMembers(member =>
        {
            SetOnlinedPlayerGuildInfo(member, 0, 5);
        });
        channelDirty = true;
    }

    public void OnEmblemChanged(short bg, byte bgcolor, short logo, byte logocolor)
    {
        this.LogoBg = bg;
        this.LogoBgColor = bgcolor;
        this.Logo = logo;
        this.LogoColor = logocolor;

        HandleCurrentServerMembers(member =>
        {
            member.sendPacket(GuildPackets.guildEmblemChange(GuildId, (short)LogoBg, (byte)LogoBgColor, (short)Logo, (byte)LogoColor));
            SetOnlinedPlayerGuildInfo(member, -1, -1);
        });

        _serverContainer.GuildManager.GetAllianceById(AllianceId)?.BroadcastGuildAlliance();
    }


    public void OnCapacityChanged()
    {
        Capacity += 5;

        this.broadcast(GuildPackets.guildCapacityChange(GuildId, this.Capacity));
    }

    public void OnGPGained(int amount)
    {
        GP += amount;

        this.broadcast(GuildPackets.updateGP(GuildId, GP));
        this.broadcast(PacketCreator.getGPMessage(amount));
    }

    public void OnGPLosed(int amount)
    {
        GP -= amount;

        this.broadcast(GuildPackets.updateGP(GuildId, GP));
    }


    public void JoinAlliance()
    {
        foreach (var mgc in Members)
        {
            var chr = _serverContainer.FindPlayerById(mgc.Key);
            if (chr != null)
            {
                chr.setAllianceRank(chr.getGuildRank() == 1 ? 2 : 5);
            }
            mgc.Value.AllianceRank = 5;
            if (mgc.Value.GuildRank == 1)
                mgc.Value.AllianceRank = 2;

        }
    }

    public void BroadcastDisplay()
    {
        HandleCurrentServerMembers(chr =>
        {
            chr.getMap().broadcastPacket(chr, GuildPackets.guildNameChanged(chr.getId(), this.Name));
            chr.getMap().broadcastPacket(chr, GuildPackets.guildMarkChanged(chr.getId(), this));
        });
    }

    #region  Guild Broadcast
    void broadcast(Packet packet, int except = -1)
    {
        HandleCurrentServerMembers(member =>
        {
            if (member.Id == except)
                return;

            member.sendPacket(packet);
        });
    }


    void dropMessage(string message)
    {
        dropMessage(5, message);
    }

    /// <summary>
    /// 如果直接调用，只会修改本机上的数据。所有操作应该由GuildManager调用
    /// </summary>
    /// <param name="type"></param>
    /// <param name="message"></param>
    void dropMessage(int type, string message)
    {
        HandleCurrentServerMembers(m =>
        {
            m.TypedMessage(type, message);
        });
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mc"></param>
    /// <param name="guildid">-1. 更新, 0. 解散</param>
    /// <param name="rank"></param>
    private void SetOnlinedPlayerGuildInfo(IPlayer mc, int guildid, int rank)
    {
        bool bDifferentGuild;
        if (guildid == -1)
        {
            bDifferentGuild = true;
        }
        else
        {
            bDifferentGuild = guildid != mc.GuildId;
            mc.GuildId = guildid;
            mc.GuildRank = rank;

            if (bDifferentGuild)
            {
                mc.AllianceRank = 5;
            }
        }
        if (bDifferentGuild)
        {
            if (mc.isLoggedinWorld())
            {
                if (guildid != 0)
                {
                    mc.getMap().broadcastPacket(mc, GuildPackets.guildNameChanged(mc.Id, Name));
                    mc.getMap().broadcastPacket(mc, GuildPackets.guildMarkChanged(mc.Id, LogoBg, LogoBgColor, Logo, LogoColor));
                }
                else
                {
                    mc.getMap().broadcastPacket(mc, GuildPackets.guildNameChanged(mc.Id, ""));
                }
            }
        }
    }

    #region Alliance
    public class Alliance
    {
        ILogger log;
        public ConcurrentDictionary<int, Guild> Guilds { get; init; }

        public int AllianceId { get; }
        public int Capacity { get; private set; }
        public string Name { get; private set; }
        public string Notice { get; private set; }
        public string[] RankTitles { get; private set; }

        public Alliance(int id, string name, int capacity, ConcurrentDictionary<int, Guild> guilds, string[] rankTitles, string notice)
        {
            AllianceId = id;
            Name = name;
            Capacity = capacity;
            Notice = notice;

            Guilds = guilds;
            RankTitles = rankTitles;
            log = LogFactory.GetLogger($"Alliance/{new RangeNumberGenerator(AllianceId, 1000)}");
        }


        public bool RemoveGuildFromAlliance(int guildId, int method)
        {
            if (method == 1 && GetLeaderGuildId() == guildId)
            {
                return false;
            }

            if (Guilds.TryRemove(guildId, out var guild))
            {
                guild.AllianceId = 0;

                broadcastMessage(GuildPackets.removeGuildFromAlliance(this, guild), -1, -1);

                BroadcastGuildAlliance();
                BroadcastNotice();
                guild.broadcast(GuildPackets.disbandAlliance(AllianceId));

                if (method == 1)
                    dropMessage("[" + guild.Name + "] guild has left the union.");
                else if (method == 2)
                    dropMessage("[" + guild.Name + "] guild has been expelled from the union.");
                return true;
            }
            return false;
            //throw new BusinessException($"GuildId {guildId} not found or not in alliance");
        }

        public void updateAlliancePackets()
        {
            if (AllianceId > 0)
            {
                BroadcastAllianceInfo();
                BroadcastNotice();
            }
        }

        public void OnGuildJoinAlliance(Guild guild)
        {
            var r = Guilds.TryAdd(guild.GuildId, guild);
            if (r)
            {
                guild.AllianceId = AllianceId;
                guild.JoinAlliance();

                broadcastMessage(GuildPackets.addGuildToAlliance(this, guild));
                updateAlliancePackets();

                guild.dropMessage("Your guild has joined the [" + Name + "] union.");
            }
        }


        public void OnDisband()
        {
            Guilds.Clear();

            broadcastMessage(GuildPackets.disbandAlliance(AllianceId), -1, -1);
        }

        public void OnRankTitleChanged(string[] ranks)
        {
            RankTitles = ranks;

            broadcastMessage(GuildPackets.changeAllianceRankTitle(AllianceId, ranks), -1, -1);
        }

        public void OnLeaderChanged(int newLeaderId, WorldChannelServer serverContainer)
        {
            var oldLeader = GetLeader();
            oldLeader.AllianceRank = 2;

            var oldLeaderObj = serverContainer.FindPlayerById(oldLeader.Id);
            if (oldLeaderObj != null)
                oldLeaderObj.setAllianceRank(2);

            var newLeader = GetMemberById(newLeaderId);
            newLeader.AllianceRank = 1;
            var newLeaderObj = serverContainer.FindPlayerById(newLeader.Id);
            if (newLeaderObj != null)
                newLeaderObj.setAllianceRank(1);

            BroadcastGuildAlliance();
            dropMessage("'" + newLeader.Name + "' has been appointed as the new head of this Alliance.");
        }
        public void OnMemberRankChanged(int cid, int newRank, WorldChannelServer serverContainer)
        {
            var targetPlayer = GetMemberById(cid);
            targetPlayer.AllianceRank = newRank;
            var targetPlayerObj = serverContainer.FindPlayerById(targetPlayer.Id);
            if (targetPlayerObj != null)
                targetPlayerObj.setAllianceRank(newRank);

            BroadcastGuildAlliance();
        }

        public string getRankTitle(int rank)
        {
            return RankTitles[rank - 1];
        }

        public List<int> getGuilds()
        {
            return Guilds.Keys.ToList();
        }

        public void OnNoticeChanged(string notice)
        {
            this.Notice = notice;

            BroadcastNotice();

            dropMessage(5, "* Alliance Notice : " + notice);
        }

        public void OnCapacityIncreased(int inc)
        {
            this.Capacity += inc;

            BroadcastGuildAlliance();
            BroadcastNotice();
        }

        public int GetLeaderGuildId()
        {
            foreach (var guild in Guilds.Values)
            {
                if (guild.getMembers().Any(x => x.AllianceRank == 1))
                    return guild.GuildId;
            }
            throw new BusinessException($"Alliance (Id = {AllianceId}) Leader not found"); ;
        }

        public int GetLeaderId()
        {
            foreach (var guild in Guilds.Values)
            {
                var leader = guild.getMembers().FirstOrDefault(x => x.AllianceRank == 1);
                if (leader != null)
                    return leader.Id;
            }
            throw new BusinessException($"Alliance (Id = {AllianceId}) Leader not found"); ;
        }

        public GuildMember GetLeader()
        {
            foreach (var guild in Guilds.Values)
            {
                var leader = guild.getMembers().FirstOrDefault(x => x.AllianceRank == 1);
                if (leader != null)
                    return leader;
            }
            throw new BusinessException($"Alliance (Id = {AllianceId}) Leader not found"); ;
        }

        public GuildMember GetMemberById(int cid)
        {
            foreach (var guild in Guilds.Values)
            {
                var leader = guild.getMembers().FirstOrDefault(x => x.Id == cid);
                if (leader != null)
                    return leader;
            }
            throw new BusinessException($"Alliance (Id = {AllianceId}) Leader not found"); ;
        }

        internal void dropMessage(string message)
        {
            dropMessage(5, message);
        }

        internal void dropMessage(int type, string message)
        {
            foreach (var guild in Guilds.Values)
            {
                guild.dropMessage(type, message);
            }
        }

        internal void broadcastMessage(Packet packet, int exception = -1, int exceptedGuildId = -1)
        {
            foreach (var guild in Guilds.Values)
            {
                if (guild.GuildId != exceptedGuildId)
                    guild.broadcast(packet, exception);
            }
        }

        internal void BroadcastPlayerInfo(int chrId)
        {
            broadcastMessage(GuildPackets.sendShowInfo(AllianceId, chrId), -1, -1);
        }

        internal void BroadcastGuildAlliance()
        {
            broadcastMessage(GuildPackets.getGuildAlliances(this), -1, -1);
        }

        void BroadcastNotice()
        {
            broadcastMessage(GuildPackets.allianceNotice(AllianceId, Notice), -1, -1);
        }

        void BroadcastAllianceInfo()
        {
            broadcastMessage(GuildPackets.updateAllianceInfo(this), -1, -1);
        }
    }
    #endregion
}