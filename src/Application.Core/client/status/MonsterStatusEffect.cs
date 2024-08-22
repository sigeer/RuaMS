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


using server.life;

namespace client.status;

public class MonsterStatusEffect
{

    private Dictionary<MonsterStatus, int> stati;
    private Skill? skill;
    private MobSkill? mobskill;
    private bool monsterSkill;

    public MonsterStatusEffect(Dictionary<MonsterStatus, int> stati, Skill? skillId, MobSkill? mobskill, bool monsterSkill)
    {
        this.stati = new(stati);
        this.skill = skillId;
        this.monsterSkill = monsterSkill;
        this.mobskill = mobskill;
    }

    public Dictionary<MonsterStatus, int> getStati()
    {
        return stati;
    }

    public int setValue(MonsterStatus status, int newVal)
    {
        stati.AddOrUpdate(status, newVal);
        return newVal;
    }

    public Skill? getSkill()
    {
        return skill;
    }

    public bool isMonsterSkill()
    {
        return monsterSkill;
    }

    public void removeActiveStatus(MonsterStatus stat)
    {
        stati.Remove(stat);
    }

    public MobSkill? getMobSkill()
    {
        return mobskill;
    }
}
