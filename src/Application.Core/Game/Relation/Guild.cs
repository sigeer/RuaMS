using Application.Core.Channel;
using Application.Shared.Guild;
using AutoMapper;
using AutoMapper.Execution;
using Microsoft.EntityFrameworkCore;
using net.server;
using net.server.guild;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
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


    private static ILogger log = LogFactory.GetLogger(LogType.Guild);
    public Alliance? AllianceModel => _serverContainer.GuildManager.GetAllianceById(AllianceId);
    public bool IsValid => members.Count > 0;


    private ConcurrentDictionary<int, GuildMember> members;
    private object membersLock = new object();

    // 1 = master, 2 = jr, 5 = lowest member
    public string[] RankTitles { get; set; } = new string[5];

    private Dictionary<int, List<int>> notifications = new();
    private Dictionary<int, bool> channelDirty;

    WorldChannelServer _serverContainer;
    public Guild(WorldChannelServer serverContainer, int guildId)
    {
        _serverContainer = serverContainer;
        channelDirty = new ();
        GuildId = guildId;
        this.members = [];
    }

    /// <summary>
    /// 相当于原先的 notifications
    /// </summary>
    List<IPlayer?>? channelMembersCache;
    public List<IPlayer?> GetCurrentChannelMembers(WorldChannel server)
    {
        if (!channelDirty.GetValueOrDefault(server.getId()))
        {
            return channelMembersCache!;
        }

        // 人员发生变动、或者上下线时重算
        channelDirty[server.getId()] = true;
        return channelMembersCache = members.Keys.Select(x => server.Players.getCharacterById(x)).ToList();
    }

    private void SendChannelMember(Action<IPlayer> action)
    {
        foreach (var server in _serverContainer.Servers.Values)
        {
            foreach (var m in GetCurrentChannelMembers(server))
            {
                if (m != null)
                {
                    action(m);
                }
            }
        }
    }

    public void SetMemberChannel(int cid, int channel)
    {
        if (members.TryGetValue(cid, out var member))
        {
            member.Channel = channel;
        }
    }
    public void SetMemberLevel(int cid, int level)
    {
        if (members.TryGetValue(cid, out var member))
        {
            member.Level = level;
        }
    }

    public void SetMemberJob(int cid, int jobId)
    {
        if (members.TryGetValue(cid, out var member))
        {
            member.JobId = jobId;
        }
    }

    public void UpdateMember(GuildMember member)
    {
        if (member.GuildId != GuildId)
        {
            return;
        }
        members[member.Id] = member;
        var chr = _serverContainer.Servers[member.Channel].Players.getCharacterById(member.Id);
        if (chr != null)
        {
            chr.GuildRank = member.GuildRank;
            chr.AllianceRank = member.AllianceRank;
        }
    }

    public int getId()
    {
        return GuildId;
    }

    public int getLeaderId()
    {
        return Leader;
    }


    public int setLeaderId(int charId)
    {
        // 家族不能更换族长？
        return Leader = charId;
    }

    public int getGP()
    {
        return GP;
    }

    public int getLogo()
    {
        return Logo;
    }

    public void setLogo(int l)
    {
        Logo = l;
    }

    public int getLogoColor()
    {
        return LogoColor;
    }

    public void setLogoColor(int c)
    {
        LogoColor = (short)c;
    }

    public int getLogoBG()
    {
        return LogoBg;
    }

    public void setLogoBG(int bg)
    {
        LogoBg = bg;
    }

    public int getLogoBGColor()
    {
        return LogoBgColor;
    }

    public void setLogoBGColor(int c)
    {
        LogoBgColor = (short)c;
    }

    public string getNotice()
    {
        return Notice ?? "";
    }

    public string getName()
    {
        return Name;
    }

    public List<GuildMember> getMembers()
    {
        Monitor.Enter(membersLock);
        try
        {
            return new(members.Values);
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public int getCapacity()
    {
        return Capacity;
    }

    public long getSignature()
    {
        return Signature;
    }

    public void BroadcastDisplay()
    {
        SendChannelMember(chr =>
        {
            chr.getMap().broadcastPacket(chr, GuildPackets.guildNameChanged(chr.getId(), this.getName()));
            chr.getMap().broadcastPacket(chr, GuildPackets.guildMarkChanged(chr.getId(), this));
        });
    }

    public void broadcastNameChanged()
    {
        SendChannelMember(chr =>
        {
            Packet packet = GuildPackets.guildNameChanged(chr.getId(), this.getName());
            chr.getMap().broadcastPacket(chr, packet);
        });
    }

    public void broadcastEmblemChanged()
    {
        SendChannelMember(chr =>
        {
            Packet packet = GuildPackets.guildMarkChanged(chr.getId(), this);
            chr.getMap().broadcastPacket(chr, packet);
        });
    }

    public void broadcastInfoChanged()
    {
        SendChannelMember(chr =>
        {
            chr.sendPacket(GuildPackets.showGuildInfo(chr));
        });
    }

    public void broadcast(Packet? packet, int exceptionId = -1, BCOp bcop = BCOp.NONE)
    {
        Monitor.Enter(membersLock); // membersLock awareness thanks to ProjectNano dev team
        try
        {
            try
            {
                SendChannelMember(member =>
                {
                    if (member != null && member.Id != exceptionId && member.isLoggedinWorld())
                    {
                        if (bcop == BCOp.DISBAND)
                        {
                            SetOnlinedPlayerGuildInfo(member, 0, 5);
                        }
                        else if (bcop == BCOp.EMBLEMCHANGE)
                        {
                            member.sendPacket(GuildPackets.guildEmblemChange(GuildId, (short)LogoBg, (byte)LogoBgColor, (short)Logo, (byte)LogoColor));
                            SetOnlinedPlayerGuildInfo(member, -1, -1);    //respawn player
                        }
                        else
                        {
                            member.sendPacket(packet!);
                        }
                    }
                });
            }
            catch (Exception re)
            {
                log.Error(re, "Failed to contact channel(s) for broadcast.");
            }
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public void guildMessage(Packet serverNotice)
    {
        broadcast(serverNotice);
    }

    public void dropMessage(string message)
    {
        dropMessage(5, message);
    }
    
    /// <summary>
    /// 如果直接调用，只会修改本机上的数据。所有操作应该由GuildManager调用
    /// </summary>
    /// <param name="type"></param>
    /// <param name="message"></param>
    public void dropMessage(int type, string message)
    {
        guildMessage(PacketCreator.serverNotice(type, message));
    }

    public void setOnline(int cid, bool online, int channel)
    {
        Monitor.Enter(membersLock);
        try
        {
            this.broadcast(GuildPackets.guildMemberOnline(GuildId, cid, online), cid);

            channelDirty[channel] = true;
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
            if (members.Count >= Capacity)
            {
                return false;
            }

            if (members.TryAdd(member.Id, member))
            {
                SendChannelMember(m =>
                {
                    if (m.Id == member.Id)
                    {
                        SetOnlinedPlayerGuildInfo(m, GuildId, 5);
                    }
                    m.sendPacket(GuildPackets.newGuildMember(GuildId, member));
                });
                channelDirty[member.Channel] = true;
                return true;
            }

            return false;
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public bool LeaveGuild(int cid)
    {
        Monitor.Enter(membersLock);
        try
        {
            if (members.TryRemove(cid, out var member))
            {
                SendChannelMember(m =>
                {
                    if (m.Id == cid)
                    {
                        SetOnlinedPlayerGuildInfo(m, 0, 5);
                    }
                    m.sendPacket(GuildPackets.memberLeft(m, false));
                });
                channelDirty[member.Channel] = true;
                return true;
            }
            return false;
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public bool ExpelMember(int cid)
    {
        Monitor.Enter(membersLock);
        try
        {
            if (members.TryRemove(cid, out var member))
            {
                SendChannelMember(m =>
                {
                    if (m.Id == cid)
                    {
                        SetOnlinedPlayerGuildInfo(m, 0, 5);
                    }
                    m.sendPacket(GuildPackets.memberLeft(m, true));
                });
                channelDirty[member.Channel] = true;
                log.Warning("Unable to find member with id {CharacterId}", cid);
                return true;
            }
            return false;
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }
    public bool ChangeRank(int cid, int newRank)
    {
        Monitor.Enter(membersLock);
        try
        {
            if (members.TryGetValue(cid, out var member))
            {
                member.GuildRank = newRank;

                SendChannelMember(m =>
                {
                    if (m.Id == cid)
                    {
                        SetOnlinedPlayerGuildInfo(m, GuildId, newRank);
                    }
                    m.sendPacket(GuildPackets.changeRank(GuildId, m.Id, newRank));
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

    public void setGuildNotice(string notice)
    {
        this.Notice = notice;

        Monitor.Enter(membersLock);
        try
        {
            this.broadcast(GuildPackets.guildNotice(GuildId, notice));
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public override bool Equals(object? other)
    {
        if (other is Guild o)
        {
            return (o.getId() == GuildId && o.getName().Equals(Name));
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

    public void changeRankTitle(string[] ranks)
    {
        Array.ConstrainedCopy(ranks, 0, RankTitles, 0, 5);

        Monitor.Enter(membersLock);
        try
        {
            this.broadcast(GuildPackets.rankTitleChange(GuildId, ranks));
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public void disbandGuild()
    {
        Monitor.Enter(membersLock);
        try
        {
            members.Clear();
            this.broadcast(null, -1, BCOp.DISBAND);
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public void setGuildEmblem(short bg, byte bgcolor, short logo, byte logocolor)
    {
        this.LogoBg = bg;
        this.LogoBgColor = bgcolor;
        this.Logo = logo;
        this.LogoColor = logocolor;

        Monitor.Enter(membersLock);
        try
        {
            this.broadcast(null, -1, BCOp.EMBLEMCHANGE);

            BroadcastDisplay();
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }


    public bool increaseCapacity()
    {
        if (Capacity > 99)
        {
            return false;
        }
        Capacity += 5;

        Monitor.Enter(membersLock);
        try
        {
            this.broadcast(GuildPackets.guildCapacityChange(GuildId, this.Capacity));
        }
        finally
        {
            Monitor.Exit(membersLock);
        }

        return true;
    }

    public void gainGP(int amount)
    {
        GP += amount;
        this.guildMessage(GuildPackets.updateGP(GuildId, GP));
        this.guildMessage(PacketCreator.getGPMessage(amount));
    }

    public void removeGP(int amount)
    {
        GP -= amount;
        this.guildMessage(GuildPackets.updateGP(GuildId, GP));
    }

    public int getAllianceId()
    {
        return AllianceId;
    }

    public void JoinAlliance()
    {
        foreach (var mgc in members)
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mc"></param>
    /// <param name="guildid">-1. 更新, 0. 解散</param>
    /// <param name="rank"></param>
    private void SetOnlinedPlayerGuildInfo(IPlayer mc, int guildid, int rank)
    {
        bool bDifferentGuild;
        if (guildid == -1 && rank == -1)
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
}
