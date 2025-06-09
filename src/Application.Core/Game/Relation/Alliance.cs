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


using Application.Core.Channel;
using Microsoft.EntityFrameworkCore;
using net.server.guild;
using System.Collections.Concurrent;

namespace Application.Core.Game.Relation;

/**
 * @author XoticStory
 * @author Ronan
 */
public class Alliance
{
    ILogger log;
    public ConcurrentDictionary<int, Guild> Guilds { get; }


    public int AllianceId { get; set; }
    public int Capacity { get; set; }
    public string Name { get; set; } = null!;
    public string Notice { get; set; }
    public string[] RankTitles { get; set; }

    public Alliance(int id)
    {
        AllianceId = id;
        RankTitles = new string[5] { "Master", "Jr. Master", "Member", "Member", "Member" };
        Notice = string.Empty;

        Guilds = [];
        log = LogFactory.GetLogger($"Alliance/{new RangeNumberGenerator(AllianceId, 1000)}");
    }


    public void saveToDB()
    {
        using var dbContext = new DBContext();
        using var dbTrans = dbContext.Database.BeginTransaction();
        dbContext.Alliances.Where(x => x.Id == AllianceId)
            .ExecuteUpdate(x => x.SetProperty(y => y.Capacity, this.Capacity)
                    .SetProperty(y => y.Notice, this.Notice)
                    .SetProperty(y => y.Rank1, RankTitles[0])
                    .SetProperty(y => y.Rank2, RankTitles[1])
                    .SetProperty(y => y.Rank3, RankTitles[2])
                    .SetProperty(y => y.Rank4, RankTitles[3])
                    .SetProperty(y => y.Rank5, RankTitles[4]));


        dbContext.AllianceGuilds.Where(x => x.AllianceId == AllianceId).ExecuteDelete();
        dbContext.AllianceGuilds.AddRange(Guilds.Keys.Select(x => new Allianceguild()
        {
            AllianceId = AllianceId,
            GuildId = x
        }));
        dbContext.SaveChanges();
        dbTrans.Commit();
    }


    public bool RemoveGuildFromAlliance(int guildId, int method)
    {
        if (method == 1 && GetLeaderGuildId() == guildId)
        {
            return false;
        }

        if (!Guilds.TryGetValue(guildId, out var guild) || guild == null)
            throw new BusinessException($"GuildId {guildId} not found or not in alliance");

        broadcastMessage(GuildPackets.removeGuildFromAlliance(this, guild), -1, -1);
        removeGuild(guildId);

        BroadcastGuildAlliance();
        BroadcastNotice();
        guild.broadcast(GuildPackets.disbandAlliance(getId()));

        if (method == 1)
            dropMessage("[" + guild.Name + "] guild has left the union.");
        else if (method == 2)
            dropMessage("[" + guild.Name + "] guild has been expelled from the union.");
        return true;
    }

    public void updateAlliancePackets()
    {
        if (AllianceId > 0)
        {
            this.BroadcastAllianceInfo();
            this.BroadcastNotice();
        }
    }

    private bool removeGuild(int gid)
    {
        var r = Guilds.TryRemove(gid, out var guild);
        if (r && guild != null)
            guild.AllianceId = 0;
        return r;
    }


    public bool TryAddGuild(Guild guild)
    {
        if (Guilds.Count == Capacity || Guilds.ContainsKey(guild.GuildId))
        {
            return false;
        }

        if (guild != null)
        {
            var r = Guilds.TryAdd(guild.GuildId, guild);
            if (r)
                guild.AllianceId = AllianceId;

            guild.resetAllianceGuildPlayersRank();
            return r;
        }
        return false;
    }


    public void Disband()
    {
        Guilds.Clear();

        broadcastMessage(GuildPackets.disbandAlliance(AllianceId), -1, -1);
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
        return Guilds.Keys.ToList();
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

    public void UpdateMember(GuildMember member)
    {
        Guilds[member.GuildId].UpdateMember(member);
    }

    public void dropMessage(string message)
    {
        dropMessage(5, message);
    }

    public void dropMessage(int type, string message)
    {
        foreach (var guild in Guilds.Values)
        {
            guild.dropMessage(type, message);
        }
    }

    public void broadcastMessage(Packet packet, int exception = -1, int exceptedGuildId = -1)
    {
        foreach (var guild in Guilds.Values)
        {
            if (guild.GuildId != exceptedGuildId)
                guild.broadcast(packet, exception);
        }
    }

    public void BroadcastPlayerInfo(int chrId)
    {
        broadcastMessage(GuildPackets.sendShowInfo(getId(), chrId), -1, -1);
    }

    public void BroadcastGuildAlliance()
    {
        broadcastMessage(GuildPackets.getGuildAlliances(this), -1, -1);
    }

    public void BroadcastNotice()
    {
        broadcastMessage(GuildPackets.allianceNotice(getId(), getNotice()), -1, -1);
    }

    public void BroadcastAllianceInfo()
    {
        broadcastMessage(GuildPackets.updateAllianceInfo(this), -1, -1);
    }
}
