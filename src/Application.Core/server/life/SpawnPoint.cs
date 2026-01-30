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
using Application.Core.Game.Life.Monsters;
using Application.Core.Game.Maps;

namespace server.life;

public class SpawnPoint
{
    private int monster;
    private int mobTime;
    private int team;
    private int fh;
    private int f;
    private int cy;
    private int rx0;
    private int rx1;
    private bool hide;
    private Point pos;
    private long nextPossibleSpawn;
    private int mobInterval = 5000;
    private AtomicInteger spawnedMonsters = new AtomicInteger(0);
    private bool denySpawn = false;
    readonly MonsterCore _monsterMeta;
    protected readonly IMap _map;

    SpawnPointTrigger act;
    public bool CanInitialSpawn => mobTime == -1;

    public SpawnPoint(
        IMap map,
        int mobId,
        Point pos, int cy, int f, int fh, int rx0, int rx1, bool hide,
        int team,
        int mobTime, int mobInterval,
        SpawnPointTrigger act = SpawnPointTrigger.Killed)
    {
        _map = map;
        _monsterMeta = LifeFactory.Instance.getMonsterStats(mobId)!;

        this.monster = mobId;
        this.pos = pos;
        this.mobTime = mobTime;
        this.team = team;
        this.cy = cy;
        this.rx0 = rx0;
        this.rx1 = rx1;
        this.fh = fh;
        this.f = f;
        this.hide = hide;
        this.mobInterval = mobInterval;
        this.nextPossibleSpawn = _map.ChannelServer.Node.getCurrentTime();
        this.act = act;
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
        if (denySpawn || mobTime < 0 || spawnedMonsters.get() >= GetMaxMobCount())
        {
            return false;
        }
        return nextPossibleSpawn <= _map.ChannelServer.Node.getCurrentTime();
    }

    /// <summary>
    /// 跳过CD，禁用情况 生成
    /// </summary>
    /// <returns></returns>
    public bool shouldForceSpawn()
    {
        return mobTime >= 0 && spawnedMonsters.get() < GetMaxMobCount();
    }

    protected virtual void SetMonsterPosition(Monster mob)
    {
        mob.setPosition(pos);
    }

    protected virtual void SubscribeMonster(Monster mob)
    {

    }

    public Monster GenrateMonster()
    {
        // Check. 原代码中，只在初始化(loadLife)的Monster中传入了fh, f, rx...等属性，这里额外加上不知道有没有问题
        var mob = LifeFactory.Instance.GetMonsterTrust(monster);
        SetMonsterPosition(mob);
        mob.setTeam(team);
        mob.setFh(fh);
        mob.setF(f);
        mob.setCy(cy);
        mob.setRx0(rx0);
        mob.setRx1(rx1);
        mob.setHide(hide);
        SubscribeMonster(mob);
        spawnedMonsters.incrementAndGet();

        if (this.act == SpawnPointTrigger.Killed)
        {
            mob.OnKilled += (sender, args) =>
            {
                nextPossibleSpawn = _map.ChannelServer.Node.getCurrentTime();
                if (mobTime > 0)
                {
                    nextPossibleSpawn += mobTime * 1000;
                }
                else
                {
                    nextPossibleSpawn += args.DieAni;
                }
                spawnedMonsters.decrementAndGet();
            };
        }

        else if (this.act == SpawnPointTrigger.Cleared)
        {
            mob.OnLifeCleared += (self, revivedMob) =>
            {
                nextPossibleSpawn = _map.ChannelServer.Node.getCurrentTime();
                if (mobTime > 0)
                {
                    nextPossibleSpawn += mobTime * 1000;
                }
                else
                {
                    nextPossibleSpawn += mobInterval;
                }
                spawnedMonsters.decrementAndGet();
            };
        }

        if (mobTime == 0)
        {
            nextPossibleSpawn = _map.ChannelServer.Node.getCurrentTime() + mobInterval;
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

    private int GetMaxMobCount()
    {
        var rate = _map.ActualMonsterRate;
        if (_map.getEventInstance() != null || _monsterMeta.Stats.isBoss())
            rate = 1;

        // 比如2.5倍，那么就算已有2只也算作满怪
        return (int)Math.Floor(rate);
    }

    public void SpawnMonster(int difficulty = 1, bool isPq = false)
    {
        var rate = _map.ActualMonsterRate;
        if (_map.getEventInstance() != null || _monsterMeta.Stats.isBoss())
            rate = 1;

        while (rate > Randomizer.NextFloat())
        {
            _map.spawnMonster(GenrateMonster(), difficulty, isPq);
            rate -= 1;
        }
    }
}
