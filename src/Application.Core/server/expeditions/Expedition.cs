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
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Shared.Events;
using System.Collections.Concurrent;
using tools;

namespace server.expeditions;

/**
 * @author Alan (SharpAceX)
 */
public class Expedition
{
    private ILogger _log = LogFactory.GetLogger(LogType.Expedition);

    private static int[] EXPEDITION_BOSSES = {
            MobId.ZAKUM_1,
            MobId.ZAKUM_2,
            MobId.ZAKUM_3,
            MobId.ZAKUM_ARM_1,
            MobId.ZAKUM_ARM_2,
            MobId.ZAKUM_ARM_3,
            MobId.ZAKUM_ARM_4,
            MobId.ZAKUM_ARM_5,
            MobId.ZAKUM_ARM_6,
            MobId.ZAKUM_ARM_7,
            MobId.ZAKUM_ARM_8,
            MobId.HORNTAIL_PREHEAD_LEFT,
            MobId.HORNTAIL_PREHEAD_RIGHT,
            MobId.HORNTAIL_HEAD_A,
            MobId.HORNTAIL_HEAD_B,
            MobId.HORNTAIL_HEAD_C,
            MobId.HORNTAIL_HAND_LEFT,
            MobId.HORNTAIL_HAND_RIGHT,
            MobId.HORNTAIL_WINGS,
            MobId.HORNTAIL_LEGS,
            MobId.HORNTAIL_TAIL,
            MobId.SCARLION_STATUE,
            MobId.SCARLION,
            MobId.ANGRY_SCARLION,
            MobId.FURIOUS_SCARLION,
            MobId.TARGA_STATUE,
            MobId.TARGA,
            MobId.ANGRY_TARGA,
            MobId.FURIOUS_TARGA,
    };

    private IPlayer leader;
    private ExpeditionType type;
    private bool registering;
    private IMap startMap;
    private List<string> bossLogs;
    private ScheduledFuture? schedule;
    private ConcurrentDictionary<int, string> members = new();
    private List<int> banned = new();
    private DateTimeOffset startTime;
    private ConcurrentDictionary<string, string> props = new();
    private bool silent;
    private int minSize;
    private int maxSize;

    public Expedition(IPlayer player, ExpeditionType met, bool sil, int minPlayers, int maxPlayers)
    {
        leader = player;
        members.AddOrUpdate(player.getId(), player.getName());
        startMap = player.getMap();
        type = met;
        silent = sil;
        minSize = (minPlayers != 0) ? minPlayers : type.getMinSize();
        maxSize = (maxPlayers != 0) ? maxPlayers : type.getMaxSize();
        bossLogs = new();
    }

    public int getMinSize()
    {
        return minSize;
    }

    public int getMaxSize()
    {
        return maxSize;
    }

    public void beginRegistration()
    {
        registering = true;
        leader.sendPacket(PacketCreator.getClock(60 * (type.getRegistrationMinutes())));
        if (!silent)
        {
            startMap.broadcastMessage(leader, PacketCreator.serverNotice(6, "[Expedition] " + leader.getName() + " has been declared the expedition captain. Please register for the expedition."), false);
            leader.sendPacket(PacketCreator.serverNotice(6, "[Expedition] You have become the expedition captain. Gather enough people for your team then talk to the NPC to start."));
        }
        scheduleRegistrationEnd();
    }

    private void scheduleRegistrationEnd()
    {
        Expedition exped = this;
        startTime = leader.Client.CurrentServerContainer.GetCurrentTimeDateTimeOffSet().AddMinutes(type.getRegistrationMinutes());

        schedule = leader.Client.CurrentServerContainer.TimerManager.schedule(() =>
        {
            if (registering)
            {
                exped.removeChannelExpedition(startMap.getChannelServer());
                if (!silent)
                {
                    startMap.broadcastMessage(PacketCreator.serverNotice(6, "[Expedition] The time limit has been reached. Expedition has been disbanded."));
                }

                dispose(false);
            }
        }, TimeSpan.FromMinutes(type.getRegistrationMinutes()));
    }

    public void dispose(bool needLog)
    {
        broadcastExped(PacketCreator.removeClock());

        if (schedule != null)
        {
            schedule.cancel(false);
        }
        if (needLog && !registering)
        {
            log();
        }
    }

    private void log()
    {
        string gmMessage = type + " Expedition with leader " + leader.getName() + " finished after " + TimeUtils.GetTimeString(startTime);
        getLeader().Client.CurrentServerContainer.SendBroadcastWorldGMPacket(PacketCreator.serverNotice(6, gmMessage));

        string log = type + " EXPEDITION\r\n";
        log += TimeUtils.GetTimeString(startTime) + "\r\n";

        foreach (string memberName in getMembers().Values)
        {
            log += ">>" + memberName + "\r\n";
        }
        log += "BOSS KILLS\r\n";
        foreach (string message in bossLogs)
        {
            log += message;
        }
        log += "\r\n";

        _log.Information(log);
    }


    public void finishRegistration()
    {
        registering = false;
    }

    public void start()
    {
        finishRegistration();
        registerExpeditionAttempt();
        broadcastExped(PacketCreator.removeClock());
        if (!silent)
        {
            broadcastExped(PacketCreator.serverNotice(6, "[Expedition] The expedition has started! Good luck, brave heroes!"));
        }
        startTime = leader.Client.CurrentServerContainer.GetCurrentTimeDateTimeOffSet();
        startMap.ChannelServer.Container.SendBroadcastWorldGMPacket(PacketCreator.serverNotice(6, "[Expedition] " + type.ToString() + " Expedition started with leader: " + leader.getName()));
    }

    public string addMember(IPlayer player)
    {
        if (!registering)
        {
            return "Sorry, this expedition is already underway. Registration is closed!";
        }
        if (banned.Contains(player.getId()))
        {
            return "Sorry, you've been banned from this expedition by #b" + leader.getName() + "#k.";
        }
        if (members.Count >= this.getMaxSize())
        { //Would be a miracle if anybody ever saw this
            return "Sorry, this expedition is full!";
        }

        var currentServer = this.getRecruitingMap().getChannelServer();
        int channel = currentServer.getId();
        if (!currentServer.Container.ExpeditionService.CanStartExpedition(player.getId(), channel, getType().name()))
        {
            // thanks Conrad, Cato for noticing some expeditions have entry limit
            return "Sorry, you've already reached the quota of attempts for this expedition! Try again another day...";
        }

        members.AddOrUpdate(player.getId(), player.getName());
        player.sendPacket(PacketCreator.getClock((startTime - leader.Client.CurrentServerContainer.GetCurrentTimeDateTimeOffSet()).Seconds));
        if (!silent)
        {
            broadcastExped(PacketCreator.serverNotice(6, "[Expedition] " + player.getName() + " has joined the expedition!"));
        }
        return "You have registered for the expedition successfully!";
    }

    public int addMemberInt(IPlayer player)
    {
        if (!registering)
        {
            return 1; //"Sorry, this expedition is already underway. Registration is closed!";
        }
        if (banned.Contains(player.getId()))
        {
            return 2; //"Sorry, you've been banned from this expedition by #b" + leader.getName() + "#k.";
        }
        if (members.Count >= this.getMaxSize())
        { //Would be a miracle if anybody ever saw this
            return 3; //"Sorry, this expedition is full!";
        }

        members.AddOrUpdate(player.getId(), player.getName());
        player.sendPacket(PacketCreator.getClock((startTime - leader.Client.CurrentServerContainer.GetCurrentTimeDateTimeOffSet()).Seconds));
        if (!silent)
        {
            broadcastExped(PacketCreator.serverNotice(6, "[Expedition] " + player.getName() + " has joined the expedition!"));
        }
        return 0; //"You have registered for the expedition successfully!";
    }

    private void registerExpeditionAttempt()
    {
        var currentServer = this.getRecruitingMap().getChannelServer();
        int channel = currentServer.getId();

        currentServer.Container.ExpeditionService.RegisterExpedition(getActiveMembers().Select(x => x.getId()).ToArray(), channel, this.getType().name());
    }

    private void broadcastExped(Packet packet)
    {
        foreach (IPlayer chr in getActiveMembers())
        {
            chr.sendPacket(packet);
        }
    }

    public bool removeMember(IPlayer chr)
    {
        if (members.Remove(chr.getId(), out var d) && d != null)
        {
            chr.sendPacket(PacketCreator.removeClock());
            if (!silent)
            {
                broadcastExped(PacketCreator.serverNotice(6, "[Expedition] " + chr.getName() + " has left the expedition."));
                chr.dropMessage(6, "[Expedition] You have left this expedition.");
            }
            return true;
        }

        return false;
    }

    //public void ban(CharacterIdNamePair chr)
    //{
    //    int cid = chr.Id;
    //    if (!banned.Contains(cid))
    //    {
    //        banned.Add(cid);
    //        members.Remove(cid);

    //        if (!silent)
    //        {
    //            broadcastExped(PacketCreator.serverNotice(6, "[Expedition] " + chr.Name + " has been banned from the expedition."));
    //        }

    //        var player = startMap.getWorldServer().getPlayerStorage().getCharacterById(cid);
    //        if (player != null && player.isLoggedinWorld())
    //        {
    //            player.sendPacket(PacketCreator.removeClock());
    //            if (!silent)
    //            {
    //                player.dropMessage(6, "[Expedition] You have been banned from this expedition.");
    //            }
    //            if (ExpeditionType.ARIANT.Equals(type) || ExpeditionType.ARIANT1.Equals(type) || ExpeditionType.ARIANT2.Equals(type))
    //            {
    //                player.changeMap(MapId.ARPQ_LOBBY);
    //            }
    //        }
    //    }
    //}

    public void monsterKilled(IPlayer chr, Monster mob)
    {
        foreach (int expeditionBoss in EXPEDITION_BOSSES)
        {
            if (mob.getId() == expeditionBoss)
            {
                bossLogs.Add(">" + mob.getName() + " was killed after " + TimeUtils.GetTimeString(startTime) + " - " + leader.Client.CurrentServerContainer.GetCurrentTimeDateTimeOffSet().ToString("HH:mm:ss") + "\r\n");
                return;
            }
        }
    }

    public void setProperty(string key, string value)
    {
        props.AddOrUpdate(key, value);
    }

    public string? getProperty(string key)
    {
        return props.GetValueOrDefault(key);
    }

    public ExpeditionType getType()
    {
        return type;
    }

    public List<IPlayer> getActiveMembers()
    {
        // thanks MedicOP for figuring out an issue with broadcasting packets to offline members
        var ps = startMap.ChannelServer.getPlayerStorage();

        List<IPlayer> activeMembers = new();
        foreach (int chrid in getMembers().Keys)
        {
            var chr = ps.getCharacterById(chrid);
            if (chr != null && chr.isLoggedinWorld())
            {
                activeMembers.Add(chr);
            }
        }

        return activeMembers;
    }

    public Dictionary<int, string> getMembers()
    {
        return new(members);
    }

    public List<CharacterIdNamePair> getMemberList()
    {
        return getMembers().OrderByDescending(x => isLeader(x.Key)).ThenBy(x => x.Key).Select(x => new CharacterIdNamePair(x.Key, x.Value)).ToList();
    }

    public bool isExpeditionTeamTogether()
    {
        List<IPlayer> chars = getActiveMembers();
        if (chars.Count <= 1)
        {
            return true;
        }

        return chars.GroupBy(x => x.getMapId()).Count() <= 1;
    }

    public void warpExpeditionTeam(int warpFrom, int warpTo)
    {
        List<IPlayer> players = getActiveMembers();

        foreach (IPlayer chr in players)
        {
            if (chr.getMapId() == warpFrom)
            {
                chr.changeMap(warpTo);
            }
        }
    }

    public void warpExpeditionTeam(int warpTo)
    {
        List<IPlayer> players = getActiveMembers();

        foreach (IPlayer chr in players)
        {
            chr.changeMap(warpTo);
        }
    }

    public void warpExpeditionTeamToMapSpawnPoint(int warpFrom, int warpTo, int toSp)
    {
        List<IPlayer> players = getActiveMembers();

        foreach (IPlayer chr in players)
        {
            if (chr.getMapId() == warpFrom)
            {
                chr.changeMap(warpTo, toSp);
            }
        }
    }

    public void warpExpeditionTeamToMapSpawnPoint(int warpTo, int toSp)
    {
        List<IPlayer> players = getActiveMembers();

        foreach (IPlayer chr in players)
        {
            chr.changeMap(warpTo, toSp);
        }
    }

    public bool addChannelExpedition(WorldChannel ch)
    {
        return ch.addExpedition(this);
    }

    public void removeChannelExpedition(WorldChannel ch)
    {
        ch.removeExpedition(this);
    }

    public IPlayer getLeader()
    {
        return leader;
    }

    public IMap getRecruitingMap()
    {
        return startMap;
    }

    public bool contains(IPlayer player)
    {
        return members.ContainsKey(player.getId()) || isLeader(player);
    }

    public bool isLeader(IPlayer player)
    {
        return isLeader(player.getId());
    }

    public bool isLeader(int playerid)
    {
        return leader.getId() == playerid;
    }

    public bool isRegistering()
    {
        return registering;
    }

    public bool isInProgress()
    {
        return !registering;
    }

    public List<string> getBossLogs()
    {
        return bossLogs;
    }
}
