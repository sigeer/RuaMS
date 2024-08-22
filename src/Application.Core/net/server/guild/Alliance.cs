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
using net.server.coordinator.world;
using net.server.world;
using Serilog;

namespace net.server.guild;

/**
 * @author XoticStory
 * @author Ronan
 */
public class Alliance
{
    static ILogger log = LogFactory.GetLogger("Alliance");
    private List<int> guilds = new();

    private int allianceId = -1;
    private int capacity;
    private string name;
    private string notice = "";
    private string[] rankTitles = new string[5];

    public Alliance(string name, int id)
    {
        this.name = name;
        allianceId = id;
        string[] ranks = { "Master", "Jr. Master", "Member", "Member", "Member" };
        for (int i = 0; i < 5; i++)
        {
            rankTitles[i] = ranks[i];
        }
    }

    public static bool canBeUsedAllianceName(string name)
    {
        if (name.Contains(" ") || name.Length > 12)
        {
            return false;
        }

        using var dbContext = new DBContext();
        return dbContext.Alliances.Any(x => x.Name == name);
    }

    private static List<Character> getPartyGuildMasters(Party party)
    {
        List<Character> mcl = new();

        foreach (PartyCharacter mpc in party.getMembers())
        {
            Character chr = mpc.getPlayer();
            if (chr != null)
            {
                Character lchr = party.getLeader().getPlayer();
                if (chr.getGuildRank() == 1 && lchr != null && chr.getMapId() == lchr.getMapId())
                {
                    mcl.Add(chr);
                }
            }
        }

        if (mcl.Count > 0 && !mcl.get(0).isPartyLeader())
        {
            for (int i = 1; i < mcl.Count; i++)
            {
                if (mcl.get(i).isPartyLeader())
                {
                    Character temp = mcl.get(0);
                    mcl.set(0, mcl.get(i));
                    mcl.set(i, temp);
                }
            }
        }

        return mcl;
    }

    public static Alliance? createAlliance(Party party, string name)
    {
        List<Character> guildMasters = getPartyGuildMasters(party);
        if (guildMasters.Count != 2)
        {
            return null;
        }

        List<int> guilds = new();
        foreach (Character mc in guildMasters)
        {
            guilds.Add(mc.getGuildId());
        }
        Alliance alliance = Alliance.createAllianceOnDb(guilds, name);
        if (alliance != null)
        {
            alliance.setCapacity(guilds.Count);
            foreach (int g in guilds)
            {
                alliance.addGuild(g);
            }

            int id = alliance.getId();
            try
            {
                for (int i = 0; i < guildMasters.Count; i++)
                {
                    Server.getInstance().setGuildAllianceId(guilds.get(i), id);
                    Server.getInstance().resetAllianceGuildPlayersRank(guilds.get(i));

                    Character chr = guildMasters.get(i);
                    chr.getMGC().setAllianceRank((i == 0) ? 1 : 2);
                    Server.getInstance().getGuild(chr.getGuildId()).getMGC(chr.getId()).setAllianceRank((i == 0) ? 1 : 2);
                    chr.saveGuildStatus();
                }

                Server.getInstance().addAlliance(id, alliance);

                int worldid = guildMasters.get(0).getWorld();
                Server.getInstance().allianceMessage(id, GuildPackets.updateAllianceInfo(alliance, worldid), -1, -1);
                Server.getInstance().allianceMessage(id, GuildPackets.getGuildAlliances(alliance, worldid), -1, -1);  // thanks Vcoc for noticing guilds from other alliances being visually stacked here due to this not being updated
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return null;
            }
        }

        return alliance;
    }

    public static Alliance createAllianceOnDb(List<int> guilds, string name)
    {
        // will create an alliance, where the first guild listed is the leader and the alliance name MUST BE already checked for unicity.

        int id = -1;
        using var dbContext = new DBContext();
        using var dbTrans = dbContext.Database.BeginTransaction();
        var newModel = new DB_Alliance(name);
        dbContext.Alliances.Add(newModel);
        dbContext.SaveChanges();
        dbContext.AllianceGuilds.AddRange(guilds.Select(x => new Allianceguild
        {
            AllianceId = newModel.Id,
            GuildId = x
        }));
        dbContext.SaveChanges();
        dbTrans.Commit();
        return new Alliance(name, id);
    }

    public static Alliance? loadAlliance(int id)
    {
        if (id <= 0)
        {
            return null;
        }
        Alliance alliance = new Alliance(null, -1);
        try
        {

            using var dbContext = new DBContext();
            var dbModel = dbContext.Alliances.Where(x => x.Id == id).FirstOrDefault();
            if (dbModel == null)
                return null;


            alliance.allianceId = id;
            alliance.capacity = dbModel.Capacity;
            alliance.name = dbModel.Name;
            alliance.notice = dbModel.Notice;

            alliance.rankTitles = new string[] { dbModel.Rank1, dbModel.Rank2, dbModel.Rank3, dbModel.Rank4, dbModel.Rank5 };

            var guilds = dbContext.AllianceGuilds.Where(x => x.AllianceId == dbModel.Id).Select(x => x.GuildId).ToList();
            guilds.ForEach(x =>
            {
                alliance.addGuild(x);
            });
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }

        return alliance;
    }

    public void saveToDB()
    {
        using var dbContext = new DBContext();
        using var dbTrans = dbContext.Database.BeginTransaction();
        dbContext.Alliances.Where(x => x.Id == allianceId).ExecuteUpdate(x => x.SetProperty(y => y.Capacity, this.capacity)
                    .SetProperty(y => y.Notice, this.notice)
                    .SetProperty(y => y.Rank1, rankTitles[0])
                    .SetProperty(y => y.Rank2, rankTitles[1])
                    .SetProperty(y => y.Rank3, rankTitles[2])
                    .SetProperty(y => y.Rank4, rankTitles[3])
                    .SetProperty(y => y.Rank5, rankTitles[4]));


        dbContext.AllianceGuilds.Where(x => x.AllianceId == allianceId).ExecuteDelete();
        dbContext.AllianceGuilds.AddRange(guilds.Select(x => new Allianceguild()
        {
            AllianceId = allianceId,
            GuildId = x
        }));
        dbContext.SaveChanges();
        dbTrans.Commit();
    }

    public static void disbandAlliance(int allianceId)
    {
        try
        {

            using var dbContext = new DBContext();
            dbContext.Alliances.Where(x => x.Id == allianceId).ExecuteDelete();
            dbContext.AllianceGuilds.Where(x => x.AllianceId == allianceId).ExecuteDelete();

            Server.getInstance().allianceMessage(allianceId, GuildPackets.disbandAlliance(allianceId), -1, -1);
            Server.getInstance().disbandAlliance(allianceId);
        }
        catch (Exception sqle)
        {
            log.Error(sqle.ToString());
        }
    }

    private static void removeGuildFromAllianceOnDb(int guildId)
    {
        try
        {
            using var dbContext = new DBContext();
            dbContext.AllianceGuilds.Where(x => x.GuildId == guildId).ExecuteDelete();
        }
        catch (Exception sqle)
        {
            log.Error(sqle.ToString());
        }
    }

    public static bool removeGuildFromAlliance(int allianceId, int guildId, int worldId)
    {
        Server srv = Server.getInstance();
        Alliance? alliance = srv.getAlliance(allianceId);

        if (alliance.getLeader().getGuildId() == guildId)
        {
            return false;
        }

        srv.allianceMessage(alliance.getId(), GuildPackets.removeGuildFromAlliance(alliance, guildId, worldId), -1, -1);
        srv.removeGuildFromAlliance(alliance.getId(), guildId);
        removeGuildFromAllianceOnDb(guildId);

        srv.allianceMessage(alliance.getId(), GuildPackets.getGuildAlliances(alliance, worldId), -1, -1);
        srv.allianceMessage(alliance.getId(), GuildPackets.allianceNotice(alliance.getId(), alliance.getNotice()), -1, -1);
        srv.guildMessage(guildId, GuildPackets.disbandAlliance(alliance.getId()));

        alliance.dropMessage("[" + srv.getGuild(guildId, worldId).getName() + "] guild has left the union.");
        return true;
    }

    public void updateAlliancePackets(Character chr)
    {
        if (allianceId > 0)
        {
            this.broadcastMessage(GuildPackets.updateAllianceInfo(this, chr.getWorld()));
            this.broadcastMessage(GuildPackets.allianceNotice(this.getId(), this.getNotice()));
        }
    }

    public bool removeGuild(int gid)
    {
        lock (guilds)
        {
            int index = getGuildIndex(gid);
            if (index == -1)
            {
                return false;
            }

            guilds.Remove(index);
            return true;
        }
    }

    public bool addGuild(int gid)
    {
        lock (guilds)
        {
            if (guilds.Count == capacity || getGuildIndex(gid) > -1)
            {
                return false;
            }

            guilds.Add(gid);
            return true;
        }
    }

    private int getGuildIndex(int gid)
    {
        lock (guilds)
        {
            for (int i = 0; i < guilds.Count; i++)
            {
                if (guilds.get(i) == gid)
                {
                    return i;
                }
            }
            return -1;
        }
    }

    public void setRankTitle(string[] ranks)
    {
        rankTitles = ranks;
    }

    public string getRankTitle(int rank)
    {
        return rankTitles[rank - 1];
    }

    public List<int> getGuilds()
    {
        lock (guilds)
        {
            List<int> guilds_ = new();
            foreach (int guild in guilds)
            {
                if (guild != -1)
                {
                    guilds_.Add(guild);
                }
            }
            return guilds_;
        }
    }

    public string getAllianceNotice()
    {
        return notice;
    }

    public string getNotice()
    {
        return notice;
    }

    public void setNotice(string notice)
    {
        this.notice = notice;
    }

    public void increaseCapacity(int inc)
    {
        this.capacity += inc;
    }

    public void setCapacity(int newCapacity)
    {
        this.capacity = newCapacity;
    }

    public int getCapacity()
    {
        return this.capacity;
    }

    public int getId()
    {
        return allianceId;
    }

    public string getName()
    {
        return name;
    }

    public GuildCharacter getLeader()
    {
        lock (guilds)
        {
            foreach (int gId in guilds)
            {
                Guild? guild = Server.getInstance().getGuild(gId);
                GuildCharacter mgc = guild.getMGC(guild.getLeaderId());

                if (mgc.getAllianceRank() == 1)
                {
                    return mgc;
                }
            }

            return null;
        }
    }

    public void dropMessage(string message)
    {
        dropMessage(5, message);
    }

    public void dropMessage(int type, string message)
    {
        lock (guilds)
        {
            foreach (int gId in guilds)
            {
                Guild guild = Server.getInstance().getGuild(gId);
                guild.dropMessage(type, message);
            }
        }
    }

    public void broadcastMessage(Packet packet)
    {
        Server.getInstance().allianceMessage(allianceId, packet, -1, -1);
    }

    public static void sendInvitation(Client c, string targetGuildName, int allianceId)
    {
        Guild? mg = Server.getInstance().getGuildByName(targetGuildName);
        if (mg == null)
        {
            c.getPlayer().dropMessage(5, "The entered guild does not exist.");
        }
        else
        {
            if (mg.getAllianceId() > 0)
            {
                c.getPlayer().dropMessage(5, "The entered guild is already registered on a guild alliance.");
            }
            else
            {
                Character victim = mg.getMGC(mg.getLeaderId()).getCharacter();
                if (victim == null)
                {
                    c.getPlayer().dropMessage(5, "The master of the guild that you offered an invitation is currently not online.");
                }
                else
                {
                    if (InviteCoordinator.createInvite(InviteType.ALLIANCE, c.getPlayer(), allianceId, victim.getId()))
                    {
                        victim.sendPacket(GuildPackets.allianceInvite(allianceId, c.getPlayer()));
                    }
                    else
                    {
                        c.getPlayer().dropMessage(5, "The master of the guild that you offered an invitation is currently managing another invite.");
                    }
                }
            }
        }
    }

    public static bool answerInvitation(int targetId, string targetGuildName, int allianceId, bool answer)
    {
        InviteResult res = InviteCoordinator.answerInvite(InviteType.ALLIANCE, targetId, allianceId, answer);

        string msg;
        Character sender = res.from;
        switch (res.result)
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

        if (sender != null)
        {
            sender.dropMessage(5, msg);
        }

        return false;
    }
}
