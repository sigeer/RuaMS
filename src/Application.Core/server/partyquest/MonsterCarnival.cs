

using Application.Core.scripting.Event;
using client;
using constants.String;
using net.server;
using net.server.channel;
using net.server.world;
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

    private Party p1, p2;
    private MapleMap map;
    private ScheduledFuture? timer, effectTimer, respawnTask;
    private long startTime = 0;
    private int summonsR = 0, summonsB = 0, room = 0;
    private Character leader1, leader2, team1, team2;
    private int redCP, blueCP, redTotalCP, blueTotalCP, redTimeupCP, blueTimeupCP;
    private bool cpq1;

    public MonsterCarnival(Party p1, Party p2, int mapid, bool cpq1, int room)
    {
        try
        {
            this.cpq1 = cpq1;
            this.room = room;
            this.p1 = p1;
            this.p2 = p2;
            Channel cs = Server.getInstance().getWorld(p2.getLeader().getWorld()).getChannel(p2.getLeader().getChannel());
            p1.setEnemy(p2);
            p2.setEnemy(p1);
            map = cs.getMapFactory().getDisposableMap(mapid);
            startTime = DateTimeOffset.Now.AddMinutes(10).ToUnixTimeMilliseconds();
            int redPortal = 0;
            int bluePortal = 0;
            if (map.isPurpleCPQMap())
            {
                redPortal = 2;
                bluePortal = 1;
            }
            foreach (PartyCharacter mpc in p1.getMembers())
            {
                Character mc = mpc.getPlayer();
                if (mc != null)
                {
                    mc.setMonsterCarnival(this);
                    mc.setTeam(0);
                    mc.setFestivalPoints(0);
                    mc.forceChangeMap(map, map.getPortal(redPortal));
                    mc.dropMessage(6, LanguageConstants.getMessage(mc, LanguageConstants.CPQEntry));
                    if (p1.getLeader().getId() == mc.getId())
                    {
                        leader1 = mc;
                    }
                    team1 = mc;
                }
            }
            foreach (PartyCharacter mpc in p2.getMembers())
            {
                Character mc = mpc.getPlayer();
                if (mc != null)
                {
                    mc.setMonsterCarnival(this);
                    mc.setTeam(1);
                    mc.setFestivalPoints(0);
                    mc.forceChangeMap(map, map.getPortal(bluePortal));
                    mc.dropMessage(6, LanguageConstants.getMessage(mc, LanguageConstants.CPQEntry));
                    if (p2.getLeader().getId() == mc.getId())
                    {
                        leader2 = mc;
                    }
                    team2 = mc;
                }
            }
            if (team1 == null || team2 == null)
            {
                foreach (PartyCharacter mpc in p1.getMembers())
                {
                    Character chr = mpc.getPlayer();
                    if (chr != null)
                    {
                        chr.dropMessage(5, LanguageConstants.getMessage(chr, LanguageConstants.CPQError));
                    }
                }
                foreach (PartyCharacter mpc in p2.getMembers())
                {
                    Character chr = mpc.getPlayer();
                    if (chr != null)
                    {
                        chr.dropMessage(5, LanguageConstants.getMessage(chr, LanguageConstants.CPQError));
                    }
                }
                return;
            }

            // thanks Atoot, Vcoc for noting double CPQ functional being sent to players in CPQ start

            timer = TimerManager.getInstance().schedule(() => timeUp(), TimeSpan.FromSeconds(map.getTimeDefault())); // thanks Atoot for noticing an irregular "event extended" issue here
            effectTimer = TimerManager.getInstance().schedule(() => complete(), TimeSpan.FromSeconds(map.getTimeDefault() - 10));
            respawnTask = TimerManager.getInstance().register(() => respawn(), YamlConfig.config.server.RESPAWN_INTERVAL);

            cs.initMonsterCarnival(cpq1, room);
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    private void respawn()
    {
        map.respawn();
    }

    public void playerDisconnected(int charid)
    {
        int team = -1;
        foreach (PartyCharacter mpc in leader1.getParty().getMembers())
        {
            if (mpc.getId() == charid)
            {
                team = 0;
            }
        }
        foreach (PartyCharacter mpc in leader2.getParty().getMembers())
        {
            if (mpc.getId() == charid)
            {
                team = 1;
            }
        }
        foreach (Character chrMap in map.getAllPlayers())
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
        return summonsR < map.getMaxMobs();
    }

    public void summonR()
    {
        summonsR++;
    }

    public bool canSummonB()
    {
        return summonsB < map.getMaxMobs();
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

        return teamReactors < map.getMaxReactors();
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

        return teamReactors < map.getMaxReactors();
    }

    protected void dispose(bool warpout)
    {
        Channel cs = map.getChannelServer();
        MapleMap outs;
        if (!cpq1)
        { // cpq2
            outs = cs.getMapFactory().getMap(980030010);
        }
        else
        {
            outs = cs.getMapFactory().getMap(980000010);
        }
        foreach (PartyCharacter mpc in leader1.getParty().getMembers())
        {
            Character mc = mpc.getPlayer();
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
        foreach (PartyCharacter mpc in leader2.getParty().getMembers())
        {
            Character mc = mpc.getPlayer();
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
        leader1.getParty().setEnemy(null);
        leader2.getParty().setEnemy(null);
        map.dispose();
        map = null;

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
            Channel cs = map.getChannelServer();
            if (winningTeam == 0)
            {
                foreach (PartyCharacter mpc in leader1.getParty().getMembers())
                {
                    Character mc = mpc.getPlayer();
                    if (mc != null)
                    {
                        mc.gainFestivalPoints(this.redTotalCP);
                        mc.setMonsterCarnival(null);
                        if (cpq1)
                        {
                            mc.changeMap(cs.getMapFactory().getMap(map.getId() + 2), cs.getMapFactory().getMap(map.getId() + 2).getPortal(0));
                        }
                        else
                        {
                            mc.changeMap(cs.getMapFactory().getMap(map.getId() + 200), cs.getMapFactory().getMap(map.getId() + 200).getPortal(0));
                        }
                        mc.setTeam(-1);
                        mc.dispelDebuffs();
                    }
                }
                foreach (PartyCharacter mpc in leader2.getParty().getMembers())
                {
                    Character mc = mpc.getPlayer();
                    if (mc != null)
                    {
                        mc.gainFestivalPoints(this.blueTotalCP);
                        mc.setMonsterCarnival(null);
                        if (cpq1)
                        {
                            mc.changeMap(cs.getMapFactory().getMap(map.getId() + 3), cs.getMapFactory().getMap(map.getId() + 3).getPortal(0));
                        }
                        else
                        {
                            mc.changeMap(cs.getMapFactory().getMap(map.getId() + 300), cs.getMapFactory().getMap(map.getId() + 300).getPortal(0));
                        }
                        mc.setTeam(-1);
                        mc.dispelDebuffs();
                    }
                }
            }
            else if (winningTeam == 1)
            {
                foreach (PartyCharacter mpc in leader2.getParty().getMembers())
                {
                    Character mc = mpc.getPlayer();
                    if (mc != null)
                    {
                        mc.gainFestivalPoints(this.blueTotalCP);
                        mc.setMonsterCarnival(null);
                        if (cpq1)
                        {
                            mc.changeMap(cs.getMapFactory().getMap(map.getId() + 2), cs.getMapFactory().getMap(map.getId() + 2).getPortal(0));
                        }
                        else
                        {
                            mc.changeMap(cs.getMapFactory().getMap(map.getId() + 200), cs.getMapFactory().getMap(map.getId() + 200).getPortal(0));
                        }
                        mc.setTeam(-1);
                        mc.dispelDebuffs();
                    }
                }
                foreach (PartyCharacter mpc in leader1.getParty().getMembers())
                {
                    Character mc = mpc.getPlayer();
                    if (mc != null)
                    {
                        mc.gainFestivalPoints(this.redTotalCP);
                        mc.setMonsterCarnival(null);
                        if (cpq1)
                        {
                            mc.changeMap(cs.getMapFactory().getMap(map.getId() + 3), cs.getMapFactory().getMap(map.getId() + 3).getPortal(0));
                        }
                        else
                        {
                            mc.changeMap(cs.getMapFactory().getMap(map.getId() + 300), cs.getMapFactory().getMap(map.getId() + 300).getPortal(0));
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
        foreach (Character chrMap in map.getAllPlayers())
        {
            chrMap.dropMessage(5, LanguageConstants.getMessage(chrMap, LanguageConstants.CPQExtendTime));
        }
        startTime = DateTimeOffset.Now.AddMinutes(3).ToUnixTimeMilliseconds();

        map.broadcastMessage(PacketCreator.getClock(3 * 60));

        timer = TimerManager.getInstance().schedule(() => timeUp(), TimeSpan.FromSeconds(map.getTimeExpand()));
        effectTimer = TimerManager.getInstance().schedule(() => complete(), TimeSpan.FromSeconds(map.getTimeExpand() - 10)); // thanks Vcoc for noticing a time set issue here
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
        foreach (PartyCharacter mpc in leader1.getParty().getMembers())
        {
            Character mc = mpc.getPlayer();
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
        foreach (PartyCharacter mpc in leader2.getParty().getMembers())
        {
            Character mc = mpc.getPlayer();
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

    public Party getRed()
    {
        return p1;
    }

    public void setRed(Party p1)
    {
        this.p1 = p1;
    }

    public Party getBlue()
    {
        return p2;
    }

    public void setBlue(Party p2)
    {
        this.p2 = p2;
    }

    public Character getLeader1()
    {
        return leader1;
    }

    public void setLeader1(Character leader1)
    {
        this.leader1 = leader1;
    }

    public Character getLeader2()
    {
        return leader2;
    }

    public void setLeader2(Character leader2)
    {
        this.leader2 = leader2;
    }

    public Character getEnemyLeader(int team)
    {
        switch (team)
        {
            case 0:
                return leader2;
            case 1:
                return leader1;
        }
        return null;
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

    public MapleMap getEventMap()
    {
        return this.map;
    }
}
