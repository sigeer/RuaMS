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


using client;
using server.maps;
using tools;

namespace Application.Core.Game.Maps.AnimatedObjects;



/**
 * @author Jan
 */
public class Summon : AbstractAnimatedMapObject
{
    private IPlayer owner;
    private sbyte skillLevel;
    private int skill;
    private int hp;
    private SummonMovementType movementType;

    public Summon(IPlayer owner, int skill, Point pos, SummonMovementType movementType)
    {
        this.owner = owner;
        this.skill = skill;
        skillLevel = owner.getSkillLevel(SkillFactory.getSkill(skill));
        if (skillLevel == 0) throw new Exception();

        this.movementType = movementType;
        setPosition(pos);
    }

    public override void sendSpawnData(IClient client)
    {
        client.sendPacket(PacketCreator.spawnSummon(this, false));
    }

    public override void sendDestroyData(IClient client)
    {
        client.sendPacket(PacketCreator.removeSummon(this, true));
    }

    public IPlayer getOwner()
    {
        return owner;
    }

    public int getSkill()
    {
        return skill;
    }

    public int getHP()
    {
        return hp;
    }

    public void addHP(int delta)
    {
        hp += delta;
    }

    public SummonMovementType getMovementType()
    {
        return movementType;
    }

    public bool isStationary()
    {
        return skill == 3111002 || skill == 3211002 || skill == 5211001 || skill == 13111004;
    }

    public sbyte getSkillLevel()
    {
        return skillLevel;
    }

    public override MapObjectType getType()
    {
        return MapObjectType.SUMMON;
    }

    public bool isPuppet()
    {
        switch (skill)
        {
            case 3111002:
            case 3211002:
            case 13111004:
                return true;
        }
        return false;
    }
}
