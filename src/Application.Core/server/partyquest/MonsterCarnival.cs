

using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Core.Game.Maps.Specials;
using Application.Core.Game.Relation;
using constants.String;
using tools;

namespace server.partyquest;

/**
 * @author Drago (Dragohe4rt)
 */
public class MonsterCarnival
{
    public static int D = 3;
    public static int C = 2;
    public static int B = 1;
    public static int A = 0;

    private ICPQMap map;
    private ScheduledFuture? effectTimer, respawnTask;
    private DateTimeOffset endingTime;
    private int room = 0;
    public bool IsCompleted { get; private set; }
    public bool IsCPQ1 { get; }
    MonsterCarnivalParty redTeam;
    MonsterCarnivalParty blueTeam;
    public WorldChannel WorldChannel { get; }
    public MonsterCarnival(WorldChannel channelServer, Team p1, Team p2, int mapid, bool cpq1, int room)
    {
        WorldChannel = channelServer;
        this.IsCPQ1 = cpq1;
        this.room = room;
        // 是否可以替换成getMap？不可以，任务结束后会关闭地图，getMap不会创建新地图
        map = (WorldChannel.getMapFactory().getDisposableMap(mapid) as ICPQMap)!;

        redTeam = new MonsterCarnivalParty(this, p1, 0);
        blueTeam = new MonsterCarnivalParty(this, p2, 1);
        redTeam.SetEnemy(blueTeam);
        blueTeam.SetEnemy(redTeam);

        if (CheckMembers())
        {
            endingTime = DateTimeOffset.UtcNow.AddSeconds(map.TimeDefault);

            effectTimer = TimerManager.getInstance().schedule(() => Complete(), TimeSpan.FromSeconds(map.TimeDefault - 10));
            respawnTask = TimerManager.getInstance().register(() => respawn(), YamlConfig.config.server.RESPAWN_INTERVAL);

            WorldChannel.initMonsterCarnival(cpq1, room);
        }
        else
        {
            dispose();
        }
    }

    public bool CheckMembers()
    {
        var redTeamMembers = redTeam.Team.GetChannelMembers(WorldChannel);
        var blueTeamMembers = blueTeam.Team.GetChannelMembers(WorldChannel);
        if (redTeamMembers.Count != blueTeamMembers.Count)
        {
            foreach (var chr in redTeamMembers)
            {
                if (chr != null)
                {
                    chr.dropMessage(5, LanguageConstants.getMessage(chr, LanguageConstants.CPQError));
                }
            }
            foreach (var chr in blueTeamMembers)
            {
                if (chr != null)
                {
                    chr.dropMessage(5, LanguageConstants.getMessage(chr, LanguageConstants.CPQError));
                }
            }
            return false;
        }
        return true;
    }

    private void respawn()
    {
        if (!IsCompleted)
            map.respawn();
    }

    public void playerDisconnected(IPlayer player)
    {
        if (player.MCTeam == null)
            return;

        int team = player.MCTeam.TeamFlag;

        foreach (var chrMap in map.getAllPlayers())
        {
            if (team == -1)
            {
                team = 1;
            }
            string teamS = "";
            switch (team)
            {
                case 0:
                    teamS = LanguageConstants.getMessage(chrMap, LanguageConstants.CPQRed);
                    break;
                case 1:
                    teamS = LanguageConstants.getMessage(chrMap, LanguageConstants.CPQBlue);
                    break;
            }
            chrMap.dropMessage(5, teamS + LanguageConstants.getMessage(chrMap, LanguageConstants.CPQPlayerExit));
        }
        earlyFinish();
    }

    private void earlyFinish()
    {
        dispose(true);
    }

    public void leftParty(IPlayer player)
    {
        playerDisconnected(player);
    }

    protected void dispose()
    {
        dispose(false);
    }

    public IMap GetOutMap()
    {
        return map.getForcedReturnMap();
    }

    protected void dispose(bool warpout)
    {
        var cs = map.getChannelServer();
        IMap outs = GetOutMap();
        redTeam.Dispose(warpout);
        blueTeam.Dispose(warpout);

        if (this.effectTimer != null)
        {
            this.effectTimer.cancel(true);
            this.effectTimer = null;
        }
        if (this.respawnTask != null)
        {
            this.respawnTask.cancel(true);
            this.respawnTask = null;
        }
        map.Dispose();
        // map = null;

        cs.finishMonsterCarnival(IsCPQ1, room);
    }

    public void exit()
    {
        dispose();
    }

    public int getTimeLeftSeconds()
    {
        return (int)(endingTime - DateTimeOffset.UtcNow).TotalSeconds;
    }

    private void ExtendTime()
    {
        foreach (var chrMap in map.getAllPlayers())
        {
            chrMap.dropMessage(5, LanguageConstants.getMessage(chrMap, LanguageConstants.CPQExtendTime));
        }
        endingTime = DateTimeOffset.UtcNow.AddSeconds(map.TimeExpand);

        map.broadcastMessage(PacketCreator.getClock(map.TimeExpand));

        effectTimer = TimerManager.getInstance().schedule(() => Complete(), TimeSpan.FromSeconds(map.TimeExpand - 10)); // thanks Vcoc for noticing a time set issue here
    }
    /// <summary>
    /// 先complete  过 10s finish
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void Complete()
    {
        if (redTeam.TotalCP != blueTeam.TotalCP)
        {
            IsCompleted = true;
            map.killAllMonsters();

            bool redWin = redTeam.TotalCP > blueTeam.TotalCP;
            this.redTeam.SetResult(redWin);
            this.blueTeam.SetResult(!redWin);

            this.redTeam.PlayUI();
            this.blueTeam.PlayUI();

            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                redTeam.Finish();
                blueTeam.Finish();
                dispose();
            });
        }
        else
        {
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                ExtendTime();
            });
        }


    }

    public ICPQMap getEventMap()
    {
        return this.map;
    }
}
