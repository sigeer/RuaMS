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
using Application.Core.Game.Maps;
using Application.Core.Game.Maps.Mists;
using Application.Core.Game.Skills;
using client.status;
using net.server.services.task.channel;

namespace server.life;

/**
 * @author Danny (Leifde)
 */
public class MobSkill : ISkill
{
    private MobSkillId id;
    private int mpCon;
    private int spawnEffect;
    private int hp;
    private int x;
    private int y;
    private int count;
    private long duration;
    private long cooltime;
    private float prop;
    private Point? lt;
    private Point? rb;
    private int limit;
    private List<int> toSummon;

    private MobSkill(MobSkillType type, int level, int mpCon, int spawnEffect, int hp, int x, int y, int count,
                     long duration, long cooltime, float prop, Point? lt, Point? rb, int limit, List<int> toSummon)
    {
        this.id = new MobSkillId(type, level);
        this.mpCon = mpCon;
        this.spawnEffect = spawnEffect;
        this.hp = hp;
        this.x = x;
        this.y = y;
        this.count = count;
        this.duration = duration;
        this.cooltime = cooltime;
        this.prop = prop;
        this.lt = lt;
        this.rb = rb;
        this.limit = limit;
        this.toSummon = toSummon;
    }

    public class Builder
    {
        private MobSkillType _type;
        private int _level;
        private int _mpCon;
        private int _spawnEffect;
        private int _hp;
        private int _x;
        private int _y;
        private int _count;
        private long _duration;
        private long _cooltime;
        private float _prop;
        private Point? _lt;
        private Point? _rb;
        private int _limit;
        private List<int> _toSummon;

        public Builder(MobSkillType type, int level)
        {
            this._type = type;
            this._level = level;
        }

        public Builder mpCon(int mpCon)
        {
            this._mpCon = mpCon;
            return this;
        }

        public Builder spawnEffect(int spawnEffect)
        {
            this._spawnEffect = spawnEffect;
            return this;
        }

        public Builder hp(int hp)
        {
            this._hp = hp;
            return this;
        }

        public Builder x(int x)
        {
            this._x = x;
            return this;
        }

        public Builder y(int y)
        {
            this._y = y;
            return this;
        }

        public Builder count(int count)
        {
            this._count = count;
            return this;
        }

        public Builder duration(long duration)
        {
            this._duration = duration;
            return this;
        }

        public Builder cooltime(long cooltime)
        {
            this._cooltime = cooltime;
            return this;
        }

        public Builder prop(float prop)
        {
            this._prop = prop;
            return this;
        }

        public Builder lt(Point? lt)
        {
            this._lt = lt;
            return this;
        }

        public Builder rb(Point? rb)
        {
            this._rb = rb;
            return this;
        }

        public Builder limit(int limit)
        {
            this._limit = limit;
            return this;
        }

        public Builder toSummon(List<int> toSummon)
        {
            this._toSummon = new List<int>(toSummon);
            return this;
        }

        public MobSkill build()
        {
            return new MobSkill(_type, _level, _mpCon, _spawnEffect, _hp, _x, _y, _count, _duration, _cooltime, _prop, _lt, _rb,
                    _limit, _toSummon);
        }
    }

    public void applyDelayedEffect(Player player, Monster monster, bool skill, int animationTime)
    {
        Action toRun = () =>
        {
            if (monster.isAlive())
            {
                applyEffect(player, monster, skill, null);
            }
        };

        OverallService service = monster.getMap().getChannelServer().OverallService;
        service.registerOverallAction(monster.getMap().getId(), toRun, animationTime);
    }

    public void applyEffect(Monster monster)
    {
        applyEffect(null, monster, false, []);
    }

    // TODO: avoid output argument banishPlayersOutput
    public void applyEffect(Player? player, Monster monster, bool skill, List<Player>? banishPlayersOutput)
    {
        // See if the MobSkill is successful before doing anything
        if (!makeChanceResult())
        {
            return;
        }

        Disease? disease = null;
        Dictionary<MonsterStatus, int> stats = new();
        List<int> reflection = new();
        switch (id.type)
        {
            case MobSkillType.ATTACK_UP:
            case MobSkillType.ATTACK_UP_M:
            case MobSkillType.PAD:
                stats.AddOrUpdate(MonsterStatus.WEAPON_ATTACK_UP, x);
                break;
            case MobSkillType.MAGIC_ATTACK_UP:
            case MobSkillType.MAGIC_ATTACK_UP_M:
            case MobSkillType.MAD:
                stats.AddOrUpdate(MonsterStatus.MAGIC_ATTACK_UP, x);
                break;
            case MobSkillType.DEFENSE_UP:
            case MobSkillType.DEFENSE_UP_M:
            case MobSkillType.PDR:
                stats.AddOrUpdate(MonsterStatus.WEAPON_DEFENSE_UP, x);
                break;
            case MobSkillType.MAGIC_DEFENSE_UP:
            case MobSkillType.MAGIC_DEFENSE_UP_M:
            case MobSkillType.MDR:
                stats.AddOrUpdate(MonsterStatus.MAGIC_DEFENSE_UP, x);
                break;
            case MobSkillType.HEAL_M:
                applyHealEffect(skill, monster);
                break;
            case MobSkillType.SEAL:
                disease = Disease.SEAL;
                break;
            case MobSkillType.DARKNESS:
                disease = Disease.DARKNESS;
                break;
            case MobSkillType.WEAKNESS:
                disease = Disease.WEAKEN;
                break;
            case MobSkillType.STUN:
                disease = Disease.STUN;
                break;
            case MobSkillType.CURSE:
                disease = Disease.CURSE;
                break;
            case MobSkillType.POISON:
                disease = Disease.POISON;
                break;
            case MobSkillType.SLOW:
                disease = Disease.SLOW;
                break;
            case MobSkillType.DISPEL:
                applyDispelEffect(skill, monster, player);
                break;
            case MobSkillType.SEDUCE:
                disease = Disease.SEDUCE;
                break;
            case MobSkillType.BANISH:
                applyBanishEffect(skill, monster, player, banishPlayersOutput);
                break;
            case MobSkillType.AREA_POISON:
                spawnMonsterMist(monster);
                break;
            case MobSkillType.REVERSE_INPUT:
                disease = Disease.CONFUSE;
                break;
            case MobSkillType.UNDEAD:
                disease = Disease.ZOMBIFY;
                break;
            case MobSkillType.PHYSICAL_IMMUNE:
                if (!monster.isBuffed(MonsterStatus.MAGIC_IMMUNITY))
                {
                    stats.AddOrUpdate(MonsterStatus.WEAPON_IMMUNITY, x);
                }
                break;

            case MobSkillType.MAGIC_IMMUNE:
                if (!monster.isBuffed(MonsterStatus.WEAPON_IMMUNITY))
                {
                    stats.AddOrUpdate(MonsterStatus.MAGIC_IMMUNITY, x);
                }
                break;
            case MobSkillType.PHYSICAL_COUNTER:
                stats.AddOrUpdate(MonsterStatus.WEAPON_REFLECT, 10);
                stats.AddOrUpdate(MonsterStatus.WEAPON_IMMUNITY, 10);
                reflection.Add(x);
                break;

            case MobSkillType.MAGIC_COUNTER:
                stats.AddOrUpdate(MonsterStatus.MAGIC_REFLECT, 10);
                stats.AddOrUpdate(MonsterStatus.MAGIC_IMMUNITY, 10);
                reflection.Add(x);
                break;
            case MobSkillType.PHYSICAL_AND_MAGIC_COUNTER:
                stats.AddOrUpdate(MonsterStatus.WEAPON_REFLECT, 10);
                stats.AddOrUpdate(MonsterStatus.WEAPON_IMMUNITY, 10);
                stats.AddOrUpdate(MonsterStatus.MAGIC_REFLECT, 10);
                stats.AddOrUpdate(MonsterStatus.MAGIC_IMMUNITY, 10);
                reflection.Add(x);
                break;
            case MobSkillType.ACC:
                stats.AddOrUpdate(MonsterStatus.ACC, x);
                break;
            case MobSkillType.EVA:
                stats.AddOrUpdate(MonsterStatus.AVOID, x);
                break;
            case MobSkillType.SPEED:
                stats.AddOrUpdate(MonsterStatus.SPEED, x);
                break;
            case MobSkillType.SEAL_SKILL:
                stats.AddOrUpdate(MonsterStatus.SEAL_SKILL, x);
                break;
            case MobSkillType.SUMMON:
                summonMonsters(monster);
                break;
        }
        if (stats.Count > 0)
        {
            applyMonsterBuffs(stats, skill, monster, reflection);
        }
        if (disease != null)
        {
            applyDisease(disease, skill, monster, player);
        }
    }

    private void applyHealEffect(bool skill, Monster monster)
    {
        if (lt != null && rb != null && skill)
        {
            var objects = getObjectsInRange(monster, MapObjectType.MONSTER);
            int hps = (getX() / 1000) * (int)(950 + 1050 * Randomizer.nextDouble());
            foreach (var mons in objects)
            {
                ((Monster)mons).heal(hps, getY());
            }
        }
        else
        {
            monster.heal(getX(), getY());
        }
    }

    private void applyDispelEffect(bool skill, Monster monster, Player player)
    {
        if (lt != null && rb != null && skill)
        {
            getPlayersInRange(monster).ForEach(x => x.dispel());
        }
        else
        {
            player.dispel();
        }
    }

    private void applyBanishEffect(bool skill, Monster monster, Player player,
                                   List<Player> banishPlayersOutput)
    {
        if (lt != null && rb != null && skill)
        {
            banishPlayersOutput.AddRange(getPlayersInRange(monster));
        }
        else
        {
            banishPlayersOutput.Add(player);
        }
    }

    private void spawnMonsterMist(Monster monster)
    {
        Rectangle mistArea = calculateBoundingBox(monster.getPosition());
        var mist = new MobMist(mistArea, monster, this);
        int mistDuration = x * 100;
        monster.getMap().spawnMist(mist, mistDuration, false, false, false);
    }

    private void summonMonsters(Monster monster)
    {
        int skillLimit = this.limit;
        var map = monster.getMap();

        if (MapId.isDojo(map.getId()))
        {  // spawns in dojo should be unlimited
            skillLimit = int.MaxValue;
        }

        if (map.getSpawnedMonstersOnMap() < 80)
        {
            List<int> summons = new(toSummon);
            int summonLimit = monster.countAvailableMobSummons(summons.Count, skillLimit);
            if (summonLimit >= 1)
            {
                bool bossRushMap = MapId.isBossRush(map.getId());

                Collections.shuffle(summons);

                foreach (int mobId in summons.Take(summonLimit))
                {
                    var toSpawn = LifeFactory.Instance.getMonster(mobId);
                    if (toSpawn != null)
                    {
                        if (bossRushMap)
                        {
                            toSpawn.disableDrops();  // no littering on BRPQ pls
                        }
                        toSpawn.setPosition(monster.getPosition());
                        int ypos, xpos;
                        xpos = monster.getPosition().X;
                        ypos = monster.getPosition().Y;
                        switch (mobId)
                        {
                            case MobId.HIGH_DARKSTAR: // Pap bomb high
                                toSpawn.setFh((int)Math.Ceiling(Randomizer.nextDouble() * 19.0));
                                ypos = -590;
                                break;
                            case MobId.LOW_DARKSTAR: // Pap bomb
                                xpos = monster.getPosition().X + Randomizer.nextInt(1000) - 500;
                                if (ypos != -590)
                                {
                                    ypos = monster.getPosition().Y;
                                }
                                break;
                            case MobId.BLOODY_BOOM: //Pianus bomb
                                if (Math.Ceiling(Randomizer.nextDouble() * 5) == 1)
                                {
                                    ypos = 78;
                                    xpos = Randomizer.nextInt(5) + (Randomizer.nextInt(2) == 1 ? 180 : 0);
                                }
                                else
                                {
                                    xpos = monster.getPosition().X + Randomizer.nextInt(1000) - 500;
                                }
                                break;
                        }
                        switch (map.getId())
                        {
                            case MapId.ORIGIN_OF_CLOCKTOWER: //Pap map
                                if (xpos < -890)
                                {
                                    xpos = (int)(Math.Ceiling(Randomizer.nextDouble() * 150) - 890);
                                }
                                else if (xpos > 230)
                                {
                                    xpos = (int)(230 - Math.Ceiling(Randomizer.nextDouble() * 150));
                                }
                                break;
                            case MapId.CAVE_OF_PIANUS: // Pianus map
                                if (xpos < -239)
                                {
                                    xpos = (int)(Math.Ceiling(Randomizer.nextDouble() * 150) - 239);
                                }
                                else if (xpos > 371)
                                {
                                    xpos = (int)(371 - Math.Ceiling(Randomizer.nextDouble() * 150));
                                }
                                break;
                        }
                        toSpawn.setPosition(new Point(xpos, ypos));
                        if (toSpawn.getId() == MobId.LOW_DARKSTAR)
                        {
                            map.spawnFakeMonster(toSpawn);
                        }
                        else
                        {
                            map.spawnMonsterWithEffect(toSpawn, spawnEffect, toSpawn.getPosition());
                        }
                        monster.addSummonedMob(toSpawn);
                    }
                }
            }
        }
    }

    private void applyMonsterBuffs(Dictionary<MonsterStatus, int> stats, bool skill, Monster monster, List<int> reflection)
    {
        if (lt != null && rb != null && skill)
        {
            foreach (var mons in getObjectsInRange(monster, MapObjectType.MONSTER))
            {
                ((Monster)mons).applyMonsterBuff(stats, getX(), getDuration(), this, reflection);
            }
        }
        else
        {
            monster.applyMonsterBuff(stats, getX(), getDuration(), this, reflection);
        }
    }

    private void applyDisease(Disease disease, bool skill, Monster monster, Player player)
    {
        if (lt != null && rb != null && skill)
        {
            int i = 0;
            foreach (var character in getPlayersInRange(monster))
            {
                if (i < count)
                {
                    character.giveDebuff(disease, this);
                    i++;
                }
                else
                {
                    break;
                }
            }
        }
        else
        {
            player.giveDebuff(disease, this);
        }
    }

    private List<Player> getPlayersInRange(Monster monster)
    {
        return monster.getMap().getPlayersInRange(calculateBoundingBox(monster.getPosition()));
    }

    public MobSkillId getId()
    {
        return id;
    }

    public MobSkillType getType()
    {
        return id.type;
    }

    public int getMpCon()
    {
        return mpCon;
    }

    public int getHP()
    {
        return hp;
    }

    public int getX()
    {
        return x;
    }

    public int getY()
    {
        return y;
    }

    public long getDuration()
    {
        return duration;
    }

    public long getCoolTime()
    {
        return cooltime;
    }

    public bool makeChanceResult()
    {
        return prop == 0 || Randomizer.nextDouble() < prop;
    }

    private Rectangle calculateBoundingBox(Point posFrom)
    {
        if (lt == null || rb == null)
            return Rectangle.Empty;

        Point mylt = new Point(lt.Value.X + posFrom.X, lt.Value.Y + posFrom.Y);
        Point myrb = new Point(rb.Value.X + posFrom.X, rb.Value.Y + posFrom.Y);
        Rectangle bounds = new Rectangle(mylt.X, mylt.Y, myrb.X - mylt.X, myrb.Y - mylt.Y);
        return bounds;
    }

    private List<IMapObject> getObjectsInRange(Monster monster, MapObjectType objectType)
    {
        return monster.getMap().getMapObjectsInBox(calculateBoundingBox(monster.getPosition()), Collections.singletonList(objectType));
    }
}
