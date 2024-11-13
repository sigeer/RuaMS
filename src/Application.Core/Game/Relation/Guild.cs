using Application.Core.Game.TheWorld;
using Application.Core.Managers;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using net.packet;
using net.server;
using net.server.guild;
using service;
using tools;

namespace Application.Core.Game.Relation;

public class Guild : IGuild
{
    public int GuildId { get; set; }

    public int Leader { get; set; }

    public int GP { get; set; }

    public int Logo { get; set; }

    public short LogoColor { get; set; }

    public string Name { get; set; } = null!;

    public string Rank1Title { get; set; } = null!;

    public string Rank2Title { get; set; } = null!;

    public string Rank3Title { get; set; } = null!;

    public string Rank4Title { get; set; } = null!;

    public string Rank5Title { get; set; } = null!;

    public int Capacity { get; set; }

    public int LogoBg { get; set; }

    public short LogoBgColor { get; set; }

    public string? Notice { get; set; }

    public long Signature { get; set; }

    public int AllianceId { get; set; }


    private static ILogger log = LogFactory.GetLogger(LogType.Guild);
    public IAlliance? AllianceModel => AllAllianceStorage.GetAllianceById(AllianceId);
    public bool IsValid => members.Count > 0;

    public enum BCOp
    {
        NONE, DISBAND, EMBLEMCHANGE
    }

    private List<IPlayer> members;
    private object membersLock = new object();

    private string[] rankTitles = new string[5]; // 1 = master, 2 = jr, 5 = lowest member

    private Dictionary<int, List<int>> notifications = new();
    private bool bDirty = true;

    readonly IMapper Mapper = GlobalTools.Mapper;

    public Guild(List<IPlayer> members)
    {
        this.members = members;
    }

    private int world = -1;
    private int GetWorld()
    {
        if (world != -1)
            return world;

        return world = members.FirstOrDefault(x => x.GuildId == GuildId)!.World;
    }

    private IWorld GetWorldModel()
    {
        return Server.getInstance().getWorld(GetWorld())!;
    }

    private void buildNotifications()
    {
        if (!bDirty)
        {
            return;
        }
        HashSet<int> chs = Server.getInstance().getOpenChannels(world);
        lock (notifications)
        {
            if (notifications.Keys.Count != chs.Count)
            {
                notifications.Clear();
                foreach (int ch in chs)
                {
                    notifications.AddOrUpdate(ch, new());
                }
            }
            else
            {
                foreach (List<int> l in notifications.Values)
                {
                    l.Clear();
                }
            }
        }

        Monitor.Enter(membersLock);
        try
        {
            foreach (var mgc in members)
            {
                if (!mgc.IsOnlined)
                {
                    continue;
                }

                List<int>? chl;
                lock (notifications)
                {
                    chl = notifications.GetValueOrDefault(mgc.Channel);
                }
                if (chl != null)
                {
                    chl.Add(mgc.getId());
                }
                //Unable to connect to Channel... error was here
            }
        }
        finally
        {
            Monitor.Exit(membersLock);
        }

        bDirty = false;
    }

    public void writeToDB(bool isDisband)
    {
        try
        {
            using var dbContext = new DBContext();
            if (!isDisband)
            {
                dbContext.Guilds.Attach(Mapper.Map<GuildEntity>(this)).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
            else
            {
                dbContext.Characters.Where(x => x.GuildId == GuildId).ExecuteUpdate(x => x.SetProperty(y => y.GuildId, 0)
                        .SetProperty(y => y.GuildRank, 5));
                dbContext.Guilds.Where(x => x.GuildId == GuildId).ExecuteDelete();

                Monitor.Enter(membersLock);
                try
                {
                    this.broadcast(GuildPackets.guildDisband(GuildId));
                }
                finally
                {
                    Monitor.Exit(membersLock);
                }
            }
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
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

    public List<IPlayer> getMembers()
    {
        Monitor.Enter(membersLock);
        try
        {
            return new(members);
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

    public void broadcastNameChanged()
    {
        foreach (var chr in getMembers())
        {
            if (chr == null || !chr.isLoggedinWorld())
            {
                continue;
            }

            Packet packet = GuildPackets.guildNameChanged(chr.getId(), this.getName());
            chr.getMap().broadcastPacket(chr, packet);
        }
    }

    public void broadcastEmblemChanged()
    {
        foreach (var chr in getMembers())
        {
            if (chr == null || !chr.isLoggedinWorld())
            {
                continue;
            }

            Packet packet = GuildPackets.guildMarkChanged(chr.getId(), this);
            chr.getMap().broadcastPacket(chr, packet);
        }
    }

    public void broadcastInfoChanged()
    {
        foreach (var chr in getMembers())
        {
            if (chr == null || !chr.isLoggedinWorld())
            {
                continue;
            }

            chr.sendPacket(GuildPackets.showGuildInfo(chr));
        }
    }

    public void broadcast(Packet packet)
    {
        broadcast(packet, -1, BCOp.NONE);
    }

    public void broadcast(Packet packet, int exception)
    {
        broadcast(packet, exception, BCOp.NONE);
    }

    public void broadcast(Packet packet, int exceptionId, BCOp bcop)
    {
        Monitor.Enter(membersLock); // membersLock awareness thanks to ProjectNano dev team
        try
        {
            lock (notifications)
            {
                if (bDirty)
                {
                    buildNotifications();
                }
                try
                {
                    foreach (int b in Server.getInstance().getOpenChannels(world))
                    {
                        var data = notifications.GetValueOrDefault(b);
                        if (data?.Count > 0)
                        {
                            if (bcop == BCOp.DISBAND)
                            {
                                GetWorldModel().setGuildAndRank(data, 0, 5, exceptionId);
                            }
                            else if (bcop == BCOp.EMBLEMCHANGE)
                            {
                                GetWorldModel().changeEmblem(GuildId, data, this);
                            }
                            else
                            {
                                GetWorldModel().sendPacket(data, packet, exceptionId);
                            }
                        }
                    }
                }
                catch (Exception re)
                {
                    log.Error(re, "Failed to contact channel(s) for broadcast.");
                }
            }
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public void guildMessage(Packet serverNotice)
    {
        Monitor.Enter(membersLock);
        try
        {
            foreach (var mgc in members)
            {
                foreach (var cs in Server.getInstance().getChannelsFromWorld(world))
                {
                    if (cs.getPlayerStorage().getCharacterById(mgc.getId()) != null)
                    {
                        cs.getPlayerStorage().getCharacterById(mgc.getId())!.sendPacket(serverNotice);
                        break;
                    }
                }
            }
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public void dropMessage(string message)
    {
        dropMessage(5, message);
    }

    public void dropMessage(int type, string message)
    {
        Monitor.Enter(membersLock);
        try
        {
            foreach (var mgc in members)
            {
                mgc.dropMessage(type, message);
            }
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public void broadcastMessage(Packet packet)
    {
        broadcast(packet);
    }

    public void setOnline(int cid, bool online, int channel)
    {
        Monitor.Enter(membersLock);
        try
        {
            this.broadcast(GuildPackets.guildMemberOnline(GuildId, cid, online), cid);

            bDirty = true;
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public void guildChat(string name, int cid, string message)
    {
        Monitor.Enter(membersLock);
        try
        {
            this.broadcast(PacketCreator.multiChat(name, message, 2), cid);
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public string getRankTitle(int rank)
    {
        return rankTitles[rank - 1];
    }

    public void ReloadGuildMembers(List<IPlayer> list)
    {
        lock (membersLock)
        {
            members = list;
        }
    }

    public int addGuildMember(IPlayer chr)
    {
        Monitor.Enter(membersLock);
        try
        {
            if (members.Count >= Capacity)
            {
                return 0;
            }
            for (int i = members.Count - 1; i >= 0; i--)
            {
                var member = members[i];
                if (member.getGuildRank() < 5 || member.getName().CompareTo(chr.Name) < 0)
                {
                    members.Insert(i + 1, chr);
                    bDirty = true;
                    break;
                }
            }

            this.broadcast(GuildPackets.newGuildMember(chr));
            return 1;
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public void leaveGuild(IPlayer mgc)
    {
        Monitor.Enter(membersLock);
        try
        {
            this.broadcast(GuildPackets.memberLeft(mgc, false));
            members.Remove(mgc);
            bDirty = true;

            mgc.GuildId = 0;
            mgc.GuildRank = 5;
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public void expelMember(IPlayer initiator, string name, int cid, NoteService noteService)
    {
        Monitor.Enter(membersLock);
        try
        {
            foreach (var mgc in members)
            {
                if (mgc.getId() == cid && initiator.getGuildRank() < mgc.getGuildRank())
                {
                    this.broadcast(GuildPackets.memberLeft(mgc, true));
                    bDirty = true;
                    try
                    {
                        if (mgc.IsOnlined)
                        {
                            Server.getInstance().getWorld(mgc.getWorld()).setGuildAndRank(cid, 0, 5);
                        }
                        else
                        {
                            noteService.sendNormal("You have been expelled from the guild.", initiator.getName(), mgc.getName());
                            Server.getInstance().getWorld(mgc.getWorld()).setOfflineGuildStatus(0, 5, cid);
                        }
                    }
                    catch (Exception re)
                    {
                        log.Error(re.ToString());
                        return;
                    }
                    return;
                }
            }
            log.Warning("Unable to find member with name {Name} and id {CharacterId}", name, cid);
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public void changeRank(int cid, int newRank)
    {
        Monitor.Enter(membersLock);
        try
        {
            foreach (var mgc in members)
            {
                if (cid == mgc.getId())
                {
                    changeRank(mgc, newRank);
                    return;
                }
            }
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public void changeRank(IPlayer mgc, int newRank)
    {
        try
        {
            if (mgc.IsOnlined)
            {
                Server.getInstance().getWorld(mgc.getWorld()).setGuildAndRank(mgc.Id, GuildId, newRank);
                mgc.setGuildRank(newRank);
            }
            else
            {
                Server.getInstance().getWorld(mgc.getWorld()).setOfflineGuildStatus((short)GuildId, (byte)newRank, mgc.Id);
                mgc.setGuildRank(newRank);
            }
        }
        catch (Exception re)
        {
            log.Error(re.ToString());
            return;
        }

        Monitor.Enter(membersLock);
        try
        {
            this.broadcast(GuildPackets.changeRank(mgc));
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public void setGuildNotice(string notice)
    {
        this.Notice = notice;
        this.writeToDB(false);

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

    public void memberLevelJobUpdate(IPlayer mgc)
    {
        Monitor.Enter(membersLock);
        try
        {
            foreach (var member in members)
            {
                if (mgc.Equals(member))
                {
                    this.broadcast(GuildPackets.guildMemberLevelJobUpdate(mgc));
                    break;
                }
            }
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
        Array.ConstrainedCopy(ranks, 0, rankTitles, 0, 5);

        Monitor.Enter(membersLock);
        try
        {
            this.broadcast(GuildPackets.rankTitleChange(GuildId, ranks));
        }
        finally
        {
            Monitor.Exit(membersLock);
        }

        this.writeToDB(false);
    }

    public void disbandGuild()
    {
        if (AllianceModel != null)
        {
            if (!AllianceModel.RemoveGuildFromAlliance(GuildId, world))
            {
                AllianceModel.Disband();
            }
        }

        Monitor.Enter(membersLock);
        try
        {
            members.Clear();
            AllGuildStorage.Remove(GuildId);
            this.writeToDB(true);
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
        this.writeToDB(false);

        Monitor.Enter(membersLock);
        try
        {
            this.broadcast(null, -1, BCOp.EMBLEMCHANGE);
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public IPlayer? getMGC(int cid)
    {
        Monitor.Enter(membersLock);
        try
        {
            return members.FirstOrDefault(x => x.getId() == cid);
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
        this.writeToDB(false);

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
        this.writeToDB(false);
        this.guildMessage(GuildPackets.updateGP(GuildId, GP));
        this.guildMessage(PacketCreator.getGPMessage(amount));
    }

    public void removeGP(int amount)
    {
        GP -= amount;
        this.writeToDB(false);
        this.guildMessage(GuildPackets.updateGP(GuildId, GP));
    }

    public int getAllianceId()
    {
        return AllianceId;
    }

    public void setAllianceId(int aid)
    {
        this.AllianceId = aid;
        try
        {
            using var dbContext = new DBContext();
            dbContext.Guilds.Where(x => x.GuildId == GuildId).ExecuteUpdate(x => x.SetProperty(y => y.AllianceId, aid));
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }

    public void resetAllianceGuildPlayersRank()
    {
        Monitor.Enter(membersLock);
        try
        {
            foreach (var mgc in members)
            {
                if (mgc.IsOnlined)
                {
                    mgc.setAllianceRank(5);
                }
            }
        }
        finally
        {
            Monitor.Exit(membersLock);
        }

        using var dbContext = new DBContext();
        dbContext.Characters.Where(x => x.GuildId == GuildId).ExecuteUpdate(x => x.SetProperty(y => y.AllianceRank, 5));
    }
}
