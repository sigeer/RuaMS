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


using Application.Utility;
using Microsoft.EntityFrameworkCore;
using net.packet;
using net.server;
using net.server.guild;

namespace Application.Core.Game.Relation;

/**
 * @author XoticStory
 * @author Ronan
 */
public class Alliance : IAlliance
{
    ILogger log;
    private List<int> guilds = new();


    public int AllianceId { get; set; }
    public int Capacity { get; set; }
    public string Name { get; set; }
    public string Notice { get; set; }
    public string[] RankTitles { get; set; }

    public Alliance(string name, int id)
    {
        Name = name;
        AllianceId = id;
        RankTitles = new string[5] { "Master", "Jr. Master", "Member", "Member", "Member" };
        Notice = string.Empty;

        log = LogFactory.GetLogger($"Alliance/{new RangeNumberGenerator(AllianceId, 1000)}");
    }


    public void saveToDB()
    {
        using var dbContext = new DBContext();
        using var dbTrans = dbContext.Database.BeginTransaction();
        dbContext.Alliances.Where(x => x.Id == AllianceId).ExecuteUpdate(x => x.SetProperty(y => y.Capacity, this.Capacity)
                    .SetProperty(y => y.Notice, this.Notice)
                    .SetProperty(y => y.Rank1, RankTitles[0])
                    .SetProperty(y => y.Rank2, RankTitles[1])
                    .SetProperty(y => y.Rank3, RankTitles[2])
                    .SetProperty(y => y.Rank4, RankTitles[3])
                    .SetProperty(y => y.Rank5, RankTitles[4]));


        dbContext.AllianceGuilds.Where(x => x.AllianceId == AllianceId).ExecuteDelete();
        dbContext.AllianceGuilds.AddRange(guilds.Select(x => new Allianceguild()
        {
            AllianceId = AllianceId,
            GuildId = x
        }));
        dbContext.SaveChanges();
        dbTrans.Commit();
    }

    private void removeGuildFromAllianceOnDb(int guildId)
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

    public bool removeGuildFromAlliance(int guildId, int worldId)
    {
        Server srv = Server.getInstance();
        if (getLeader().getGuildId() == guildId)
        {
            return false;
        }

        srv.allianceMessage(getId(), GuildPackets.removeGuildFromAlliance(this, guildId, worldId), -1, -1);
        srv.removeGuildFromAlliance(getId(), guildId);
        removeGuildFromAllianceOnDb(guildId);

        srv.allianceMessage(getId(), GuildPackets.getGuildAlliances(this, worldId), -1, -1);
        srv.allianceMessage(getId(), GuildPackets.allianceNotice(getId(), getNotice()), -1, -1);
        srv.guildMessage(guildId, GuildPackets.disbandAlliance(getId()));

        dropMessage("[" + srv.getGuild(guildId).getName() + "] guild has left the union.");
        return true;
    }

    public void updateAlliancePackets(IPlayer chr)
    {
        if (AllianceId > 0)
        {
            this.broadcastMessage(GuildPackets.updateAllianceInfo(this, chr.getWorld()));
            this.broadcastMessage(GuildPackets.allianceNotice(this.getId(), this.getNotice()));
        }
    }

    public bool removeGuild(int gid)
    {
        lock (guilds)
        {
            return guilds.Remove(gid);
        }
    }

    public bool addGuild(int gid)
    {
        lock (guilds)
        {
            if (guilds.Count == Capacity || getGuildIndex(gid) > -1)
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
            return guilds.IndexOf(gid);
        }
    }

    public void setRankTitle(string[] ranks)
    {
        RankTitles = ranks;
    }

    public string getRankTitle(int rank)
    {
        return RankTitles[rank - 1];
    }

    public List<int> getGuilds()
    {
        lock (guilds)
        {
            return guilds.Where(x => x != -1).ToList();
        }
    }

    public string getAllianceNotice()
    {
        return Notice;
    }

    public string getNotice()
    {
        return Notice;
    }

    public void setNotice(string notice)
    {
        this.Notice = notice;
    }

    public void increaseCapacity(int inc)
    {
        this.Capacity += inc;
    }

    public void setCapacity(int newCapacity)
    {
        this.Capacity = newCapacity;
    }

    public int getCapacity()
    {
        return this.Capacity;
    }

    public int getId()
    {
        return AllianceId;
    }

    public string getName()
    {
        return Name;
    }

    object getLeaderLock = new object();
    public IPlayer getLeader()
    {
        lock (getLeaderLock)
        {
            foreach (int gId in guilds)
            {
                var guild = Server.getInstance().getGuild(gId)!;
                var mgc = guild.getMGC(guild.getLeaderId());

                if (mgc?.AllianceRank == 1)
                {
                    return mgc;
                }
            }

            throw new BusinessException($"Alliance (Id = {AllianceId}) Leader not found");
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
                var guild = Server.getInstance().getGuild(gId);
                guild?.dropMessage(type, message);
            }
        }
    }

    public void broadcastMessage(Packet packet)
    {
        Server.getInstance().allianceMessage(AllianceId, packet, -1, -1);
    }


}
