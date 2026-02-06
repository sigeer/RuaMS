/*
    This file is part of the HeavenMS MapleStory Server
    Copyleft (L) 2016 - 2019 RonanLana

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


using Application.Core.Channel.Commands;
using Application.Core.Game.Maps;
using Application.Shared.Events;
using server.expeditions;
using tools;

namespace server.partyquest;

/**
 * @author Ronan
 */
public class AriantColiseum
{

    private Expedition exped;
    private IMap map;

    private Dictionary<Player, int> score;
    private Dictionary<Player, int> rewardTier;
    private bool scoreDirty = false;

    private ScheduledFuture? ariantUpdate;
    private ScheduledFuture? ariantFinish;
    private ScheduledFuture? ariantScoreboard;

    private int lostShards = 0;

    private bool eventClear = false;

    public AriantColiseum(IMap eventMap, Expedition expedition)
    {
        exped = expedition;
        exped.finishRegistration();

        map = eventMap;
        map.resetFully();

        long pqTimer = 10 * 60 * 1000;
        long pqTimerBoard = 9 * 60 * 1000 + 50 * 1000;

        List<Player> players = exped.getActiveMembers();
        score = new();
        rewardTier = new();
        foreach (Player mc in players)
        {
            mc.changeMap(map, 0);
            mc.setAriantColiseum(this);
            mc.updateAriantScore();
            rewardTier.AddOrUpdate(mc, 0);
        }

        foreach (Player mc in players)
        {
            mc.sendPacket(PacketCreator.updateAriantPQRanking(score));
        }

        setAriantScoreBoard(eventMap.ChannelServer.Node.TimerManager.schedule(() =>
        {
            eventMap.ChannelServer.Post(new EventAriantClearCommand(this));
        }, pqTimerBoard));

        setArenaFinish(eventMap.ChannelServer.Node.TimerManager.schedule(() =>
        {
            eventMap.ChannelServer.Post(new EventAriantWarpOutCommand(this));
        }, pqTimer));

        setArenaUpdate(eventMap.ChannelServer.Node.TimerManager.register(() =>
        {
            eventMap.ChannelServer.Post(new EventAriantBroadcastScoreCommand(this));
        }, 500, 500));
    }

    private void setArenaUpdate(ScheduledFuture? ariantUpdate)
    {
        this.ariantUpdate = ariantUpdate;
    }

    private void setArenaFinish(ScheduledFuture? arenaFinish)
    {
        this.ariantFinish = arenaFinish;
    }

    private void setAriantScoreBoard(ScheduledFuture? ariantScore)
    {
        this.ariantScoreboard = ariantScore;
    }

    private void cancelArenaUpdate()
    {
        if (ariantUpdate != null)
        {
            ariantUpdate.cancel(true);
            ariantUpdate = null;
        }
    }

    private void cancelArenaFinish()
    {
        if (ariantFinish != null)
        {
            ariantFinish.cancel(true);
            ariantFinish = null;
        }
    }

    private void cancelAriantScoreBoard()
    {
        if (ariantScoreboard != null)
        {
            ariantScoreboard.cancel(true);
            ariantScoreboard = null;
        }
    }

    private void cancelAriantSchedules()
    {
        cancelArenaUpdate();
        cancelArenaFinish();
        cancelAriantScoreBoard();
    }

    public int getAriantScore(Player chr)
    {
        return score.GetValueOrDefault(chr);
    }

    public void clearAriantScore(Player chr)
    {
        score.Remove(chr);
    }

    public void updateAriantScore(Player chr, int points)
    {
        if (map != null)
        {
            score.AddOrUpdate(chr, points);
            scoreDirty = true;
        }
    }

    public void broadcastAriantScoreUpdate()
    {
        if (scoreDirty)
        {
            foreach (Player chr in score.Keys)
            {
                chr.sendPacket(PacketCreator.updateAriantPQRanking(score));
            }
            scoreDirty = false;
        }
    }

    public int getAriantRewardTier(Player chr)
    {
        return rewardTier.GetValueOrDefault(chr);
    }

    public void clearAriantRewardTier(Player chr)
    {
        rewardTier.Remove(chr);
    }

    public void addLostShards(int quantity)
    {
        lostShards += quantity;
    }

    public void leaveArena(Player chr)
    {
        if (!(eventClear && GameConstants.isAriantColiseumArena(chr.getMapId())))
        {
            leaveArenaInternal(chr);
        }
    }

    private void leaveArenaInternal(Player chr)
    {
        if (exped != null)
        {
            if (exped.removeMember(chr))
            {
                int minSize = eventClear ? 1 : 2;
                if (exped.getActiveMembers().Count < minSize)
                {
                    dispose();
                }
                chr.setAriantColiseum(null);

                int shards = chr.countItem(ItemId.ARPQ_SPIRIT_JEWEL);
                chr.GainItem(ItemId.ARPQ_SPIRIT_JEWEL, (short)-shards, show: GainItemShow.ShowInChat);
                chr.updateAriantScore(shards);
            }
        }
    }

    public void playerDisconnected(Player chr)
    {
        leaveArenaInternal(chr);
    }

    public void showArenaResults()
    {
        eventClear = true;

        if (map != null)
        {
            map.broadcastMessage(PacketCreator.showAriantScoreBoard());
            map.killAllMonsters();

            distributeAriantPoints();
        }
    }

    private static bool isUnfairMatch(int winnerScore, int secondScore, int lostShardsScore, List<int> runnerupsScore)
    {
        if (winnerScore <= 0)
        {
            return false;
        }

        double runnerupsScoreCount = 0;
        foreach (int i in runnerupsScore)
        {
            runnerupsScoreCount += i;
        }

        runnerupsScoreCount += lostShardsScore;
        secondScore += lostShardsScore;

        double matchRes = runnerupsScoreCount / winnerScore;
        double runnerupRes = ((double)secondScore) / winnerScore;

        return matchRes < 0.81770726891980117713114871015349 && (runnerupsScoreCount < 7 || runnerupRes < 0.5929);
    }

    public void distributeAriantPoints()
    {
        int firstTop = -1, secondTop = -1;
        Player? winner = null;
        List<int> runnerups = new();

        foreach (var e in score)
        {
            int s = e.Value;
            if (s > firstTop)
            {
                secondTop = firstTop;
                firstTop = s;
                winner = e.Key;
            }
            else if (s > secondTop)
            {
                secondTop = s;
            }

            runnerups.Add(s);
            rewardTier.AddOrUpdate(e.Key, (int)Math.Floor((double)s / 10));
        }

        runnerups.Remove(firstTop);
        if (isUnfairMatch(firstTop, secondTop, map.getDroppedItemsCountById(ItemId.ARPQ_SPIRIT_JEWEL) + lostShards, runnerups))
        {
            rewardTier.AddOrUpdate(winner!, 1);
        }
    }

    private ExpeditionType getExpeditionType()
    {
        switch (map.getId())
        {
            case MapId.ARPQ_ARENA_1:
                return ExpeditionType.ARIANT;
            case MapId.ARPQ_ARENA_2:
                return ExpeditionType.ARIANT1;
            default:
                return ExpeditionType.ARIANT2;
        }
    }

    public void enterKingsRoom()
    {
        exped.removeChannelExpedition(map.getChannelServer());
        cancelAriantSchedules();

        foreach (Player chr in map.getAllPlayers())
        {
            chr.changeMap(MapId.ARPQ_KINGS_ROOM, 0);
        }
    }

    private void dispose()
    {
        if (exped != null)
        {
            exped.dispose(false);

            foreach (Player chr in exped.getActiveMembers())
            {
                chr.setAriantColiseum(null);
                chr.changeMap(MapId.ARPQ_LOBBY, 0);
            }

            score.Clear();
        }
    }
}
