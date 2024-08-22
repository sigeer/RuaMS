/*
	This file is part of the OdinMS Maple Story Server
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
		       Matthias Butz <matze@odinms.de>
		       Jan Christian Meyer <vimes@odinms.de>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation version 3 as published by
    the Free Software Foundation. You may not use, modify or distribute
    this program under any other version of the GNU Affero General Public
    License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


using client;
using Microsoft.EntityFrameworkCore;
using net.packet;
using net.server.channel;
using net.server.coordinator.matchchecker;
using net.server.coordinator.world;
using service;
using tools;

namespace net.server.guild;



public class Guild
{
    private static ILogger log = LogFactory.GetLogger("Guild");

    public enum BCOp
    {
        NONE, DISBAND, EMBLEMCHANGE
    }

    private List<GuildCharacter> members;
    private object membersLock = new object();

    private string[] rankTitles = new string[5]; // 1 = master, 2 = jr, 5 = lowest member
    private string name, notice;
    private int id, gp, logo, logoColor, leader, capacity, logoBG, logoBGColor, allianceId;
    private long signature;
    private int world;
    private Dictionary<int, List<int>> notifications = new();
    private bool bDirty = true;

    public Guild(int guildid, int world)
    {
        this.world = world;
        members = new();

        using var dbContext = new DBContext();
        var dbModel = dbContext.Guilds.FirstOrDefault(x => x.GuildId == guildid);
        if (dbModel != null)
        {
            id = dbModel.GuildId;
            name = dbModel.Name;
            gp = dbModel.GP;
            logo = dbModel.Logo;
            logoColor = dbModel.LogoColor;
            logoBG = dbModel.LogoBg;
            logoBGColor = dbModel.LogoBgColor;
            rankTitles = new string[] { dbModel.Rank1Title, dbModel.Rank2Title, dbModel.Rank3Title, dbModel.Rank4Title, dbModel.Rank5Title };
            leader = dbModel.Leader;
            notice = dbModel.Notice;
            signature = dbModel.Signature;
            allianceId = dbModel.AllianceId;

            var list = dbContext.Characters.Where(x => x.GuildId == dbModel.GuildId).OrderBy(x => x.GuildRank).ThenBy(x => x.Name).ToList();
            members = list.Select(x => new GuildCharacter(x.Id, x.Level, x.Name, -1, x.World, x.Job, x.Rank, x.GuildId, false, x.AllianceRank)).ToList();
        }
        else
        {
            id = -1;
        }
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
            foreach (GuildCharacter mgc in members)
            {
                if (!mgc.isOnline())
                {
                    continue;
                }

                List<int>? chl;
                lock (notifications)
                {
                    chl = notifications.GetValueOrDefault(mgc.getChannel());
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

    public void writeToDB(bool bDisband)
    {
        try
        {
            using var dbContext = new DBContext();
            if (!bDisband)
            {
                var guildList = dbContext.Guilds.Where(x => x.GuildId == this.id);
                guildList.ExecuteUpdate(x => x.SetProperty(y => y.GP, gp)
                        .SetProperty(y => y.Logo, logo)
                        .SetProperty(Y => Y.LogoColor, logoColor)
                        .SetProperty(y => y.LogoBgColor, logoBGColor)
                        .SetProperty(y => y.LogoBg, logoBG)
                        .SetProperty(y => y.Rank1Title, rankTitles[0])
                        .SetProperty(y => y.Rank2Title, rankTitles[1])
                        .SetProperty(y => y.Rank3Title, rankTitles[2])
                        .SetProperty(y => y.Rank4Title, rankTitles[3])
                        .SetProperty(y => y.Rank5Title, rankTitles[4])
                        .SetProperty(y => y.Capacity, capacity)
                        .SetProperty(y => y.Notice, notice));

            }
            else
            {
                dbContext.Characters.Where(x => x.GuildId == this.id).ExecuteUpdate(x => x.SetProperty(y => y.GuildId, 0)
                        .SetProperty(y => y.GuildRank, 5));
                dbContext.Guilds.Where(x => x.GuildId == this.id).ExecuteDelete();

                Monitor.Enter(membersLock);
                try
                {
                    this.broadcast(GuildPackets.guildDisband(this.id));
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
        return id;
    }

    public int getLeaderId()
    {
        return leader;
    }

    public int setLeaderId(int charId)
    {
        return leader = charId;
    }

    public int getGP()
    {
        return gp;
    }

    public int getLogo()
    {
        return logo;
    }

    public void setLogo(int l)
    {
        logo = l;
    }

    public int getLogoColor()
    {
        return logoColor;
    }

    public void setLogoColor(int c)
    {
        logoColor = c;
    }

    public int getLogoBG()
    {
        return logoBG;
    }

    public void setLogoBG(int bg)
    {
        logoBG = bg;
    }

    public int getLogoBGColor()
    {
        return logoBGColor;
    }

    public void setLogoBGColor(int c)
    {
        logoBGColor = c;
    }

    public string getNotice()
    {
        if (notice == null)
        {
            return "";
        }
        return notice;
    }

    public string getName()
    {
        return name;
    }

    public List<GuildCharacter> getMembers()
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
        return capacity;
    }

    public long getSignature()
    {
        return signature;
    }

    public void broadcastNameChanged()
    {
        PlayerStorage ps = Server.getInstance().getWorld(world).getPlayerStorage();

        foreach (GuildCharacter mgc in getMembers())
        {
            var chr = ps.getCharacterById(mgc.getId());
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
        PlayerStorage ps = Server.getInstance().getWorld(world).getPlayerStorage();

        foreach (GuildCharacter mgc in getMembers())
        {
            var chr = ps.getCharacterById(mgc.getId());
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
        PlayerStorage ps = Server.getInstance().getWorld(world).getPlayerStorage();

        foreach (GuildCharacter mgc in getMembers())
        {
            var chr = ps.getCharacterById(mgc.getId());
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
                                Server.getInstance().getWorld(world).setGuildAndRank(data, 0, 5, exceptionId);
                            }
                            else if (bcop == BCOp.EMBLEMCHANGE)
                            {
                                Server.getInstance().getWorld(world).changeEmblem(this.id, data, new GuildSummary(this));
                            }
                            else
                            {
                                Server.getInstance().getWorld(world).sendPacket(data, packet, exceptionId);
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
            foreach (GuildCharacter mgc in members)
            {
                foreach (Channel cs in Server.getInstance().getChannelsFromWorld(world))
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
            foreach (GuildCharacter mgc in members)
            {
                if (mgc.getCharacter() != null)
                {
                    mgc.getCharacter().dropMessage(type, message);
                }
            }
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public void broadcastMessage(Packet packet)
    {
        Server.getInstance().guildMessage(id, packet);
    }

    public void setOnline(int cid, bool online, int channel)
    {
        Monitor.Enter(membersLock);
        try
        {
            bool bBroadcast = true;
            foreach (GuildCharacter mgc in members)
            {
                if (mgc.getId() == cid)
                {
                    if (mgc.isOnline() && online)
                    {
                        bBroadcast = false;
                    }
                    mgc.setOnline(online);
                    mgc.setChannel(channel);
                    break;
                }
            }
            if (bBroadcast)
            {
                this.broadcast(GuildPackets.guildMemberOnline(id, cid, online), cid);
            }
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

    public static int createGuild(int leaderId, string name)
    {
        using var dbContext = new DBContext();
        if (dbContext.Guilds.Any(x => x.Name == name))
            return 0;

        using var transaction = dbContext.Database.BeginTransaction();
        var guildModel = new DB_Guild
        {
            Leader = leaderId,
            Name = name,
            Signature = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
        };
        dbContext.Guilds.Add(guildModel);
        dbContext.SaveChanges();

        dbContext.Characters.Where(x => x.Id == leaderId).ExecuteUpdate(x => x.SetProperty(y => y.GuildId, guildModel.GuildId));
        transaction.Commit();
        return guildModel.GuildId;
    }

    public int addGuildMember(GuildCharacter mgc, Character chr)
    {
        Monitor.Enter(membersLock);
        try
        {
            if (members.Count >= capacity)
            {
                return 0;
            }
            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (members.get(i).getGuildRank() < 5 || members.get(i).getName().CompareTo(mgc.getName()) < 0)
                {
                    mgc.setCharacter(chr);
                    members.Insert(i + 1, mgc);
                    bDirty = true;
                    break;
                }
            }

            this.broadcast(GuildPackets.newGuildMember(mgc));
            return 1;
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public void leaveGuild(GuildCharacter mgc)
    {
        Monitor.Enter(membersLock);
        try
        {
            this.broadcast(GuildPackets.memberLeft(mgc, false));
            members.Remove(mgc);
            bDirty = true;
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public void expelMember(GuildCharacter initiator, string name, int cid, NoteService noteService)
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
                        if (mgc.isOnline())
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
            log.Warning("Unable to find member with name {GuildName} and id {CharacterId}", name, cid);
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
            foreach (GuildCharacter mgc in members)
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

    public void changeRank(GuildCharacter mgc, int newRank)
    {
        try
        {
            if (mgc.isOnline())
            {
                Server.getInstance().getWorld(mgc.getWorld()).setGuildAndRank(mgc.getId(), this.id, newRank);
                mgc.setGuildRank(newRank);
            }
            else
            {
                Server.getInstance().getWorld(mgc.getWorld()).setOfflineGuildStatus((short)this.id, (byte)newRank, mgc.getId());
                mgc.setOfflineGuildRank(newRank);
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
        this.notice = notice;
        this.writeToDB(false);

        Monitor.Enter(membersLock);
        try
        {
            this.broadcast(GuildPackets.guildNotice(this.id, notice));
        }
        finally
        {
            Monitor.Exit(membersLock);
        }
    }

    public void memberLevelJobUpdate(GuildCharacter mgc)
    {
        Monitor.Enter(membersLock);
        try
        {
            foreach (GuildCharacter member in members)
            {
                if (mgc.Equals(member))
                {
                    member.setJobId(mgc.getJobId());
                    member.setLevel(mgc.getLevel());
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
        if (other is GuildCharacter o)
        {
            return (o.getId() == id && o.getName().Equals(name));
        }
        return false;
    }

    public override int GetHashCode()
    {
        int hash = 3;
        hash = 89 * hash + (this.name != null ? this.name.GetHashCode() : 0);
        hash = 89 * hash + this.id;
        return hash;
    }

    public void changeRankTitle(string[] ranks)
    {
        Array.ConstrainedCopy(ranks, 0, rankTitles, 0, 5);

        Monitor.Enter(membersLock);
        try
        {
            this.broadcast(GuildPackets.rankTitleChange(this.id, ranks));
        }
        finally
        {
            Monitor.Exit(membersLock);
        }

        this.writeToDB(false);
    }

    public void disbandGuild()
    {
        if (allianceId > 0)
        {
            if (!Alliance.removeGuildFromAlliance(allianceId, id, world))
            {
                Alliance.disbandAlliance(allianceId);
            }
        }

        Monitor.Enter(membersLock);
        try
        {
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
        this.logoBG = bg;
        this.logoBGColor = bgcolor;
        this.logo = logo;
        this.logoColor = logocolor;
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

    public GuildCharacter? getMGC(int cid)
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
        if (capacity > 99)
        {
            return false;
        }
        capacity += 5;
        this.writeToDB(false);

        Monitor.Enter(membersLock);
        try
        {
            this.broadcast(GuildPackets.guildCapacityChange(this.id, this.capacity));
        }
        finally
        {
            Monitor.Exit(membersLock);
        }

        return true;
    }

    public void gainGP(int amount)
    {
        this.gp += amount;
        this.writeToDB(false);
        this.guildMessage(GuildPackets.updateGP(this.id, this.gp));
        this.guildMessage(PacketCreator.getGPMessage(amount));
    }

    public void removeGP(int amount)
    {
        this.gp -= amount;
        this.writeToDB(false);
        this.guildMessage(GuildPackets.updateGP(this.id, this.gp));
    }

    public static GuildResponse? sendInvitation(Client c, string targetName)
    {
        var mc = c.getChannelServer().getPlayerStorage().getCharacterByName(targetName);
        if (mc == null)
        {
            return GuildResponse.NOT_IN_CHANNEL;
        }
        if (mc.getGuildId() > 0)
        {
            return GuildResponse.ALREADY_IN_GUILD;
        }

        Character sender = c.getPlayer();
        if (InviteCoordinator.createInvite(InviteType.GUILD, sender, sender.getGuildId(), mc.getId()))
        {
            mc.sendPacket(GuildPackets.guildInvite(sender.getGuildId(), sender.getName()));
            return null;
        }
        else
        {
            return GuildResponse.MANAGING_INVITE;
        }
    }

    public static bool answerInvitation(int targetId, string targetName, int guildId, bool answer)
    {
        InviteResult res = InviteCoordinator.answerInvite(InviteType.GUILD, targetId, guildId, answer);

        GuildResponse mgr;
        Character sender = res.from;
        switch (res.result)
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

        if (mgr != null && sender != null)
        {
            sender.sendPacket(mgr.getPacket(targetName));
        }
        return false;
    }

    public static HashSet<Character> getEligiblePlayersForGuild(Character guildLeader)
    {
        HashSet<Character> guildMembers = new();
        guildMembers.Add(guildLeader);

        MatchCheckerCoordinator mmce = guildLeader.getWorldServer().getMatchCheckerCoordinator();
        foreach (Character chr in guildLeader.getMap().getAllPlayers())
        {
            if (chr.getParty() == null && chr.getGuild() == null && mmce.getMatchConfirmationLeaderid(chr.getId()) == -1)
            {
                guildMembers.Add(chr);
            }
        }

        return guildMembers;
    }

    public static void displayGuildRanks(Client c, int npcid)
    {
        try
        {
            using var dbContext = new DBContext();
            var rs = dbContext.Guilds.OrderByDescending(x => x.GP).Take(50).ToList();
            c.sendPacket(GuildPackets.showGuildRanks(npcid, rs));
        }
        catch (Exception e)
        {
            log.Error(e, "Failed to display guild ranks.");
        }
    }

    public int getAllianceId()
    {
        return allianceId;
    }

    public void setAllianceId(int aid)
    {
        this.allianceId = aid;
        try
        {
            using var dbContext = new DBContext();
            dbContext.Guilds.Where(x => x.GuildId == id).ExecuteUpdate(x => x.SetProperty(y => y.AllianceId, aid));
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
            foreach (GuildCharacter mgc in members)
            {
                if (mgc.isOnline())
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
        dbContext.Characters.Where(x => x.GuildId == id).ExecuteUpdate(x => x.SetProperty(y => y.AllianceRank, 5));
    }

    public static int getIncreaseGuildCost(int size)
    {
        int cost = YamlConfig.config.server.EXPAND_GUILD_BASE_COST + Math.Max(0, (size - 15) / 5) * YamlConfig.config.server.EXPAND_GUILD_TIER_COST;

        if (size > 30)
        {
            return Math.Min(YamlConfig.config.server.EXPAND_GUILD_MAX_COST, Math.Max(cost, 5000000));
        }
        else
        {
            return cost;
        }
    }
}
