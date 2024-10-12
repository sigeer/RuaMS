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


using Application.Core.Game.Life;
using net.server;

namespace server.life;

public class SpawnPoint
{
    private int monster;
    private int mobTime;
    private int team;
    private int fh;
    private int f;
    private Point pos;
    private long nextPossibleSpawn;
    private int mobInterval = 5000;
    private AtomicInteger spawnedMonsters = new AtomicInteger(0);
    private bool immobile;
    private bool denySpawn = false;

    public SpawnPoint(Monster monster, Point pos, bool immobile, int mobTime, int mobInterval, int team)
    {
        this.monster = monster.getId();
        this.pos = pos;
        this.mobTime = mobTime;
        this.team = team;
        this.fh = monster.getFh();
        this.f = monster.getF();
        this.immobile = immobile;
        this.mobInterval = mobInterval;
        this.nextPossibleSpawn = Server.getInstance().getCurrentTime();
    }

    public int getSpawned()
    {
        return spawnedMonsters;
    }

    public void setDenySpawn(bool val)
    {
        denySpawn = val;
    }

    public bool getDenySpawn()
    {
        return denySpawn;
    }

    public bool shouldSpawn()
    {
        if (denySpawn || mobTime < 0 || spawnedMonsters.get() > 0)
        {
            return false;
        }
        return nextPossibleSpawn <= Server.getInstance().getCurrentTime();
    }

    public bool shouldForceSpawn()
    {
        return mobTime >= 0 && spawnedMonsters.get() <= 0;
    }

    public Monster getMonster()
    {
        var mob = LifeFactory.GetMonsterTrust(monster);
        mob.setPosition(pos);
        mob.setTeam(team);
        mob.setFh(fh);
        mob.setF(f);
        spawnedMonsters.incrementAndGet();
        mob.addListener(new ActualMonsterListener()
        {
            monsterKilled = (int aniTime) =>
            {
                nextPossibleSpawn = Server.getInstance().getCurrentTime();
                if (mobTime > 0)
                {
                    nextPossibleSpawn += mobTime * 1000;
                }
                else
                {
                    nextPossibleSpawn += aniTime;
                }
                spawnedMonsters.decrementAndGet();
            }
        });
        if (mobTime == 0)
        {
            nextPossibleSpawn = Server.getInstance().getCurrentTime() + mobInterval;
        }
        return mob;
    }

    public int getMonsterId()
    {
        return monster;
    }

    public Point getPosition()
    {
        return pos;
    }

    public int getF()
    {
        return f;
    }

    public int getFh()
    {
        return fh;
    }

    public int getMobTime()
    {
        return mobTime;
    }

    public int getTeam()
    {
        return team;
    }
}
