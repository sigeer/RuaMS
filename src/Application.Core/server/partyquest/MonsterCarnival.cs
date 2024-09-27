

using Application.Core.Game.Maps;
using Application.Core.Game.Maps.Specials;
using Application.Core.Game.Relation;
using Application.Core.scripting.Event;
using constants.String;
using server.maps;
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

    private ITeam p1, p2;
    private ICPQMap map;
    private ScheduledFuture? timer, effectTimer, respawnTask;
    private long startTime = 0;
    private int summonsR = 0, summonsB = 0, room = 0;
    private IPlayer leader1, leader2;
    IPlayer? team1, team2;
    private int redCP, blueCP, redTotalCP, blueTotalCP, redTimeupCP, blueTimeupCP;
    private bool cpq1;

    public MonsterCarnival(ITeam p1, ITeam p2, int mapid, bool cpq1, int room)
    {
        this.cpq1 = cpq1;
        this.room = room;
        this.p1 = p1;
        this.p2 = p2;
        var cs = p2.getLeader().getChannelServer();
        p1.setEnemy(p2);
        p2.setEnemy(p1);
        // 是否可以替换成getMap？不可以，任务结束后会关闭地图，getMap不会创建新地图
        map =(cs.getMapFactory().getMap(mapid).Clone() as ICPQMap)!;
        startTime = DateTimeOffset.Now.AddMinutes(10).ToUnixTimeMilliseconds();
        int redPortal = 0;
        int bluePortal = 0;
        if (map.isPurpleCPQMap())
        {
            redPortal = 2;
            bluePortal = 1;
        }
        leader1 = p1.getLeader();
        foreach (var mc in p1.getMembers())
        {
            if (mc != null)
            {
                mc.setMonsterCarnival(this);
                mc.setTeam(0);
                mc.setFestivalPoints(0);
                mc.forceChangeMap(map, map.getPortal(redPortal));
                mc.dropMessage(6, LanguageConstants.getMessage(mc, LanguageConstants.CPQEntry));
                team1 = mc;
            }
        }

        leader2 = p2.getLeader();
        foreach (var mc in p2.getMembers())
        {
            if (mc != null)
            {
                mc.setMonsterCarnival(this);
                mc.setTeam(1);
                mc.setFestivalPoints(0);
                mc.forceChangeMap(map, map.getPortal(bluePortal));
                mc.dropMessage(6, LanguageConstants.getMessage(mc, LanguageConstants.CPQEntry));
                team2 = mc;
            }
        }
        if (team1 == null || team2 == null)
        {
            foreach (var chr in p1.getMembers())
            {
                if (chr != null)
                {
                    chr.dropMessage(5, LanguageConstants.getMessage(chr, LanguageConstants.CPQError));
                }
            }
            foreach (var chr in p2.getMembers())
            {
                if (chr != null)
                {
                    chr.dropMessage(5, LanguageConstants.getMessage(chr, LanguageConstants.CPQError));
                }
            }
            return;
        }

        // thanks Atoot, Vcoc for noting double CPQ functional being sent to players in CPQ start

        timer = TimerManager.getInstance().schedule(() => timeUp(), TimeSpan.FromSeconds(map.TimeDefault)); // thanks Atoot for noticing an irregular "event extended" issue here
        effectTimer = TimerManager.getInstance().schedule(() => complete(), TimeSpan.FromSeconds(map.TimeDefault - 10));
        respawnTask = TimerManager.getInstance().register(() => respawn(), YamlConfig.config.server.RESPAWN_INTERVAL);

        cs.initMonsterCarnival(cpq1, room);
    }

    private void respawn()
    {
        map.respawn();
    }

    public void playerDisconnected(int charid)
    {
        int team = -1;
        foreach (var mpc in leader1.getParty()!.getMembers())
        {
            if (mpc.getId() == charid)
            {
                team = 0;
            }
        }
        foreach (var mpc in leader2.getParty()!.getMembers())
        {
            if (mpc.getId() == charid)
            {
                team = 1;
            }
        }
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

    public void leftParty(int charid)
    {
        playerDisconnected(charid);
    }

    protected void dispose()
    {
        dispose(false);
    }

    public bool canSummonR()
    {
        return summonsR < map.MaxReactors;
    }

    public void summonR()
    {
        summonsR++;
    }

    public bool canSummonB()
    {
        return summonsB < map.MaxMobs;
    }

    public void summonB()
    {
        summonsB++;
    }

    public bool canGuardianR()
    {
        int teamReactors = 0;
        foreach (Reactor react in map.getAllReactors())
        {
            if (react.getName().Substring(0, 1) == ("0"))
            {
                teamReactors += 1;
            }
        }

        return teamReactors < map.MaxReactors;
    }

    public bool canGuardianB()
    {
        int teamReactors = 0;
        foreach (Reactor react in map.getAllReactors())
        {
            if (react.getName().Substring(0, 1) == ("1"))
            {
                teamReactors += 1;
            }
        }

        return teamReactors < map.MaxReactors;
    }

    protected void dispose(bool warpout)
    {
        var cs = map.getChannelServer();
        IMap outs;
        if (!cpq1)
        { // cpq2
            outs = cs.getMapFactory().getMap(980030010);
        }
        else
        {
            outs = cs.getMapFactory().getMap(980000010);
        }
        foreach (var mc in leader1.getParty()!.getMembers())
        {
            if (mc != null)
            {
                mc.resetCP();
                mc.setTeam(-1);
                mc.setMonsterCarnival(null);
                if (warpout)
                {
                    mc.changeMap(outs, outs.getPortal(0));
                }
            }
        }
        foreach (var mc in leader2.getParty()!.getMembers())
        {
            if (mc != null)
            {
                mc.resetCP();
                mc.setTeam(-1);
                mc.setMonsterCarnival(null);
                if (warpout)
                {
                    mc.changeMap(outs, outs.getPortal(0));
                }
            }
        }
        if (this.timer != null)
        {
            this.timer.cancel(true);
            this.timer = null;
        }
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
        redTotalCP = 0;
        blueTotalCP = 0;
        leader1.getParty()!.setEnemy(null);
        leader2.getParty()!.setEnemy(null);
        map.dispose();
        // map = null;

        cs.finishMonsterCarnival(cpq1, room);
    }

    public void exit()
    {
        dispose();
    }

    public ScheduledFuture? getTimer()
    {
        return this.timer;
    }

    private void finish(int winningTeam)
    {
        try
        {
            var cs = map.getChannelServer();
            var mapFactory = cs.getMapFactory();
            if (winningTeam == 0)
            {
                foreach (var mc in leader1.getParty()!.getMembers())
                {
                    if (mc != null)
                    {
                        mc.gainFestivalPoints(this.redTotalCP);
                        mc.setMonsterCarnival(null);
                        if (cpq1)
                        {
                            mc.changeMap(mapFactory.getMap(map.getId() + 2), mapFactory.getMap(map.getId() + 2).getPortal(0));
                        }
                        else
                        {
                            mc.changeMap(mapFactory.getMap(map.getId() + 200), mapFactory.getMap(map.getId() + 200).getPortal(0));
                        }
                        mc.setTeam(-1);
                        mc.dispelDebuffs();
                    }
                }
                foreach (var mc in leader2.getParty()!.getMembers())
                {
                    if (mc != null)
                    {
                        mc.gainFestivalPoints(this.blueTotalCP);
                        mc.setMonsterCarnival(null);
                        if (cpq1)
                        {
                            mc.changeMap(mapFactory.getMap(map.getId() + 3), mapFactory.getMap(map.getId() + 3).getPortal(0));
                        }
                        else
                        {
                            mc.changeMap(mapFactory.getMap(map.getId() + 300), mapFactory.getMap(map.getId() + 300).getPortal(0));
                        }
                        mc.setTeam(-1);
                        mc.dispelDebuffs();
                    }
                }
            }
            else if (winningTeam == 1)
            {
                foreach (var mc in leader2.getParty()!.getMembers())
                {
                    if (mc != null)
                    {
                        mc.gainFestivalPoints(this.blueTotalCP);
                        mc.setMonsterCarnival(null);
                        if (cpq1)
                        {
                            mc.changeMap(mapFactory.getMap(map.getId() + 2), mapFactory.getMap(map.getId() + 2).getPortal(0));
                        }
                        else
                        {
                            mc.changeMap(mapFactory.getMap(map.getId() + 200), mapFactory.getMap(map.getId() + 200).getPortal(0));
                        }
                        mc.setTeam(-1);
                        mc.dispelDebuffs();
                    }
                }
                foreach (var mc in leader1.getParty()!.getMembers())
                {
                    if (mc != null)
                    {
                        mc.gainFestivalPoints(this.redTotalCP);
                        mc.setMonsterCarnival(null);
                        if (cpq1)
                        {
                            mc.changeMap(mapFactory.getMap(map.getId() + 3), mapFactory.getMap(map.getId() + 3).getPortal(0));
                        }
                        else
                        {
                            mc.changeMap(mapFactory.getMap(map.getId() + 300), mapFactory.getMap(map.getId() + 300).getPortal(0));
                        }
                        mc.setTeam(-1);
                        mc.dispelDebuffs();
                    }
                }
            }
            dispose();
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    private void timeUp()
    {
        int cp1 = this.redTimeupCP;
        int cp2 = this.blueTimeupCP;
        if (cp1 == cp2)
        {
            extendTime();
            return;
        }
        if (cp1 > cp2)
        {
            finish(0);
        }
        else
        {
            finish(1);
        }
    }

    public long getTimeLeft()
    {
        return (startTime - DateTimeOffset.Now.ToUnixTimeMilliseconds());
    }

    public int getTimeLeftSeconds()
    {
        return (int)(getTimeLeft() / 1000);
    }

    private void extendTime()
    {
        foreach (var chrMap in map.getAllPlayers())
        {
            chrMap.dropMessage(5, LanguageConstants.getMessage(chrMap, LanguageConstants.CPQExtendTime));
        }
        startTime = DateTimeOffset.Now.AddMinutes(3).ToUnixTimeMilliseconds();

        map.broadcastMessage(PacketCreator.getClock(3 * 60));

        timer = TimerManager.getInstance().schedule(() => timeUp(), TimeSpan.FromSeconds(map.TimeExpand));
        effectTimer = TimerManager.getInstance().schedule(() => complete(), TimeSpan.FromSeconds(map.TimeExpand - 10)); // thanks Vcoc for noticing a time set issue here
    }

    public void complete()
    {
        int cp1 = this.redTotalCP;
        int cp2 = this.blueTotalCP;

        this.redTimeupCP = cp1;
        this.blueTimeupCP = cp2;

        if (cp1 == cp2)
        {
            return;
        }
        bool redWin = cp1 > cp2;
        int chnl = leader1.getClient().getChannel();
        int chnl1 = leader2.getClient().getChannel();
        if (chnl != chnl1)
        {
            throw new Exception("Os lideres estao em canais diferentes.");
        }

        map.killAllMonsters();
        foreach (var mc in leader1.getParty()!.getMembers())
        {
            if (mc != null)
            {
                if (redWin)
                {
                    mc.sendPacket(PacketCreator.showEffect("quest/carnival/win"));
                    mc.sendPacket(PacketCreator.playSound("MobCarnival/Win"));
                    mc.dispelDebuffs();
                }
                else
                {
                    mc.sendPacket(PacketCreator.showEffect("quest/carnival/lose"));
                    mc.sendPacket(PacketCreator.playSound("MobCarnival/Lose"));
                    mc.dispelDebuffs();
                }
            }
        }
        foreach (var mc in leader2.getParty()!.getMembers())
        {
            if (mc != null)
            {
                if (!redWin)
                {
                    mc.sendPacket(PacketCreator.showEffect("quest/carnival/win"));
                    mc.sendPacket(PacketCreator.playSound("MobCarnival/Win"));
                    mc.dispelDebuffs();
                }
                else
                {
                    mc.sendPacket(PacketCreator.showEffect("quest/carnival/lose"));
                    mc.sendPacket(PacketCreator.playSound("MobCarnival/Lose"));
                    mc.dispelDebuffs();
                }
            }
        }
    }

    public ITeam getRed()
    {
        return p1;
    }

    public void setRed(ITeam p1)
    {
        this.p1 = p1;
    }

    public ITeam getBlue()
    {
        return p2;
    }

    public void setBlue(ITeam p2)
    {
        this.p2 = p2;
    }

    public IPlayer getLeader1()
    {
        return leader1;
    }

    public void setLeader1(IPlayer leader1)
    {
        this.leader1 = leader1;
    }

    public IPlayer getLeader2()
    {
        return leader2;
    }

    public void setLeader2(IPlayer leader2)
    {
        this.leader2 = leader2;
    }

    public IPlayer getEnemyLeader(int team)
    {
        return team switch
        {
            0 => leader2,
            1 => leader1,
            _ => throw new BusinessException(),
        };
    }

    public int getBlueCP()
    {
        return blueCP;
    }

    public void setBlueCP(int blueCP)
    {
        this.blueCP = blueCP;
    }

    public int getBlueTotalCP()
    {
        return blueTotalCP;
    }

    public void setBlueTotalCP(int blueTotalCP)
    {
        this.blueTotalCP = blueTotalCP;
    }

    public int getRedCP()
    {
        return redCP;
    }

    public void setRedCP(int redCP)
    {
        this.redCP = redCP;
    }

    public int getRedTotalCP()
    {
        return redTotalCP;
    }

    public void setRedTotalCP(int redTotalCP)
    {
        this.redTotalCP = redTotalCP;
    }

    public int getTotalCP(int team)
    {
        if (team == 0)
        {
            return redTotalCP;
        }
        else if (team == 1)
        {
            return blueTotalCP;
        }
        else
        {
            throw new Exception("Equipe desconhecida");
        }
    }

    public void setTotalCP(int totalCP, int team)
    {
        if (team == 0)
        {
            this.redTotalCP = totalCP;
        }
        else if (team == 1)
        {
            this.blueTotalCP = totalCP;
        }
    }

    public int getCP(int team)
    {
        if (team == 0)
        {
            return redCP;
        }
        else if (team == 1)
        {
            return blueCP;
        }
        else
        {
            throw new Exception("Equipe desconhecida" + team);
        }
    }

    public void setCP(int CP, int team)
    {
        if (team == 0)
        {
            this.redCP = CP;
        }
        else if (team == 1)
        {
            this.blueCP = CP;
        }
    }

    public int getRoom()
    {
        return this.room;
    }

    public IMap getEventMap()
    {
        return this.map;
    }
}
