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
using Application.Core.Channel.Commands;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Resources.Messages;
using Application.Shared.Events;
using System;
using System.Collections.Concurrent;
using tools;

namespace server.expeditions;

/**
 * @author Alan (SharpAceX)
 */
public class Expedition : IClientMessenger
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

    private Player leader;
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

    public Expedition(Player player, ExpeditionType met, bool sil, int minPlayers, int maxPlayers)
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
            startMap.BroadcastAll(e =>
            {
                if (e != leader)
                    e.LightBlue(nameof(ClientMessage.Expedition_Captain_NoticeMap));
            });
            leader.LightBlue(nameof(ClientMessage.Expedition_Captain_Notice));
        }
        scheduleRegistrationEnd();
    }

    private void scheduleRegistrationEnd()
    {
        Expedition exped = this;
        startTime = leader.Client.CurrentServer.Node.GetCurrentTimeDateTimeOffset().AddMinutes(type.getRegistrationMinutes());

        schedule = leader.Client.CurrentServer.Node.TimerManager.schedule(() =>
        {
            leader.Client.CurrentServer.Post(new ExpeditionRegistrationTimeoutCommand(this));
        }, TimeSpan.FromMinutes(type.getRegistrationMinutes()));
    }

    public void ProcessRegistrationTimeout()
    {
        if (registering)
        {
            removeChannelExpedition(startMap.getChannelServer());
            if (!silent)
            {
                startMap.LightBlue(nameof(ClientMessage.Expedition_Timeout_Disband));
            }

            dispose(false);
        }
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
        getLeader().Client.CurrentServer.NodeActor.Post(new SendWorldBroadcastMessageCommand(6, gmMessage, true));

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
            LightBlue(nameof(ClientMessage.Expedition_Start));
        }
        startTime = leader.Client.CurrentServer.Node.GetCurrentTimeDateTimeOffset();
        startMap.ChannelServer.NodeActor.Post(new SendWorldBroadcastMessageCommand(6, "[Expedition] " + type.ToString() + " Expedition started with leader: " + leader.getName(), true));
    }

    public string addMember(Player player)
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
        {
            //Would be a miracle if anybody ever saw this
            return player.GetMessageByKey(nameof(ClientMessage.Expedition_MemberFull));
        }

        var currentServer = this.getRecruitingMap().getChannelServer();
        int channel = currentServer.getId();
        if (!currentServer.NodeService.ExpeditionService.CanStartExpedition(player.getId(), channel, getType().name()))
        {
            // thanks Conrad, Cato for noticing some expeditions have entry limit
            return "Sorry, you've already reached the quota of attempts for this expedition! Try again another day...";
        }

        members.AddOrUpdate(player.getId(), player.getName());
        player.sendPacket(PacketCreator.getClock((startTime - leader.Client.CurrentServer.Node.GetCurrentTimeDateTimeOffset()).Seconds));
        if (!silent)
        {
            LightBlue(nameof(ClientMessage.Expedition_Join), player.Name);
        }
        return "You have registered for the expedition successfully!";
    }

    public int addMemberInt(Player player)
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
        player.sendPacket(PacketCreator.getClock((startTime - leader.Client.CurrentServer.Node.GetCurrentTimeDateTimeOffset()).Seconds));
        if (!silent)
        {
            LightBlue(nameof(ClientMessage.Expedition_Join), player.Name);
        }
        return 0; //"You have registered for the expedition successfully!";
    }

    private void registerExpeditionAttempt()
    {
        var currentServer = this.getRecruitingMap().getChannelServer();
        int channel = currentServer.getId();

        currentServer.NodeService.ExpeditionService.RegisterExpedition(getActiveMembers().Select(x => x.getId()).ToArray(), channel, this.getType().name());
    }

    private void broadcastExped(Packet packet)
    {
        foreach (Player chr in getActiveMembers())
        {
            chr.sendPacket(packet);
        }
    }

    public bool removeMember(Player chr)
    {
        if (members.Remove(chr.getId(), out var d) && d != null)
        {
            chr.sendPacket(PacketCreator.removeClock());
            if (!silent)
            {
                LightBlue(nameof(ClientMessage.Expedition_Left), chr.Name);
                chr.LightBlue(nameof(ClientMessage.Expedition_ChrLeft));
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

    public void monsterKilled(Player chr, Monster mob)
    {
        foreach (int expeditionBoss in EXPEDITION_BOSSES)
        {
            if (mob.getId() == expeditionBoss)
            {
                bossLogs.Add(">" + mob.getName() + " was killed after " + TimeUtils.GetTimeString(startTime) + " - " + leader.Client.CurrentServer.Node.GetCurrentTimeDateTimeOffset().ToString("HH:mm:ss") + "\r\n");
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

    public List<Player> getActiveMembers()
    {
        // thanks MedicOP for figuring out an issue with broadcasting packets to offline members
        // 没看到对频道的限制。 也没有看到切换频道后离开远征队的代码
        var ps = startMap.ChannelServer.getPlayerStorage();

        List<Player> activeMembers = new();
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
        List<Player> chars = getActiveMembers();
        if (chars.Count <= 1)
        {
            return true;
        }

        return chars.GroupBy(x => x.getMapId()).Count() <= 1;
    }

    public void warpExpeditionTeam(int warpFrom, int warpTo)
    {
        List<Player> players = getActiveMembers();

        foreach (Player chr in players)
        {
            if (chr.getMapId() == warpFrom)
            {
                chr.changeMap(warpTo);
            }
        }
    }

    public void warpExpeditionTeam(int warpTo)
    {
        List<Player> players = getActiveMembers();

        foreach (Player chr in players)
        {
            chr.changeMap(warpTo);
        }
    }

    public void warpExpeditionTeamToMapSpawnPoint(int warpFrom, int warpTo, int toSp)
    {
        List<Player> players = getActiveMembers();

        foreach (Player chr in players)
        {
            if (chr.getMapId() == warpFrom)
            {
                chr.changeMap(warpTo, toSp);
            }
        }
    }

    public void warpExpeditionTeamToMapSpawnPoint(int warpTo, int toSp)
    {
        List<Player> players = getActiveMembers();

        foreach (Player chr in players)
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

    public Player getLeader()
    {
        return leader;
    }

    public IMap getRecruitingMap()
    {
        return startMap;
    }

    public bool contains(Player player)
    {
        return members.ContainsKey(player.getId()) || isLeader(player);
    }

    public bool isLeader(Player player)
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


    public void TypedMessage(int type, string messageKey, params string[] param)
    {
        foreach (Player chr in getActiveMembers())
        {
            chr.TypedMessage(type, messageKey, param);
        }
    }

    public void Notice(string key, params string[] param)
    {
        TypedMessage(0, key, param);
    }

    public void Popup(string key, params string[] param)
    {
        TypedMessage(1, key, param);
    }

    public void Dialog(string key, params string[] param)
    {
        foreach (Player chr in getActiveMembers())
        {
            chr.Dialog(key, param);
        }
    }

    public void Pink(string key, params string[] param)
    {
        TypedMessage(5, key, param);
    }

    public void LightBlue(string key, params string[] param)
    {
        TypedMessage(6, key, param);
    }

    public void LightBlue(Func<ClientCulture, string> action)
    {
        foreach (var chr in getActiveMembers())
        {
            chr.LightBlue(action);
        }
    }

    public void TopScrolling(string key, params string[] param)
    {
        foreach (var chr in getActiveMembers())
        {
            chr.TopScrolling(key, param);
        }
    }

    public void Yellow(string key, params string[] param)
    {
        foreach (Player chr in getActiveMembers())
        {
            chr.Yellow(key, param);
        }
    }
}
