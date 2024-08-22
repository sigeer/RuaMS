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
using constants.skills;
using net.packet;
using server.life;
using tools;

namespace server.maps;



/**
 * @author LaiLaiNoob
 */
public class Mist : AbstractMapObject
{
    private Rectangle mistPosition;
    private Character? owner = null;
    private Monster? mob = null;
    private StatEffect? source;
    private MobSkill skill;
    private bool _isMobMist, _isPoisonMist, _isRecoveryMist;
    private int skillDelay;

    public Mist(Rectangle mistPosition, Monster mob, MobSkill skill)
    {
        this.mistPosition = mistPosition;
        this.mob = mob;
        this.skill = skill;
        _isMobMist = true;
        _isPoisonMist = true;
        _isRecoveryMist = false;
        skillDelay = 0;
    }

    public Mist(Rectangle mistPosition, Character owner, StatEffect source)
    {
        this.mistPosition = mistPosition;
        this.owner = owner;
        this.source = source;
        this.skillDelay = 8;
        this._isMobMist = false;
        this._isRecoveryMist = false;
        this._isPoisonMist = false;
        switch (source.getSourceId())
        {
            case Evan.RECOVERY_AURA:
                _isRecoveryMist = true;
                break;

            case Shadower.SMOKE_SCREEN: // Smoke Screen
                _isPoisonMist = false;
                break;

            case FPMage.POISON_MIST: // FP mist
            case BlazeWizard.FLAME_GEAR: // Flame Gear
            case NightWalker.POISON_BOMB: // Poison Bomb
                _isPoisonMist = true;
                break;
        }
    }

    public override MapObjectType getType()
    {
        return MapObjectType.MIST;
    }

    public override Point getPosition()
    {
        return mistPosition.Location;
    }

    public Skill getSourceSkill()
    {
        return SkillFactory.getSkill(source.getSourceId());
    }

    public bool isMobMist()
    {
        return _isMobMist;
    }

    public bool isPoisonMist()
    {
        return _isPoisonMist;
    }

    public bool isRecoveryMist()
    {
        return _isRecoveryMist;
    }

    public int getSkillDelay()
    {
        return skillDelay;
    }

    public Monster getMobOwner()
    {
        return mob;
    }

    public Character getOwner()
    {
        return owner;
    }

    public Rectangle getBox()
    {
        return mistPosition;
    }

    public override void setPosition(Point position)
    {
        throw new NotImplementedException();
    }

    public Packet makeDestroyData()
    {
        return PacketCreator.removeMist(getObjectId());
    }

    public Packet makeSpawnData()
    {
        if (owner != null)
        {
            return PacketCreator.spawnMist(getObjectId(), owner.getId(), getSourceSkill().getId(), owner.getSkillLevel(SkillFactory.getSkill(source.getSourceId())), this);
        }
        return PacketCreator.spawnMobMist(getObjectId(), mob.getId(), skill.getId(), this);
    }

    public Packet makeFakeSpawnData(int level)
    {
        if (owner != null)
        {
            return PacketCreator.spawnMist(getObjectId(), owner.getId(), getSourceSkill().getId(), level, this);
        }
        return PacketCreator.spawnMobMist(getObjectId(), mob.getId(), skill.getId(), this);
    }

    public override void sendSpawnData(Client client)
    {
        client.sendPacket(makeSpawnData());
    }

    public override void sendDestroyData(Client client)
    {
        client.sendPacket(makeDestroyData());
    }

    public bool makeChanceResult()
    {
        return source.makeChanceResult();
    }
}
