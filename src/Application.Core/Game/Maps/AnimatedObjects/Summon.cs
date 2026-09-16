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


using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Skills;
using Application.Core.Server;
using Application.Shared.MapObjects.Summons;
using Application.Templates.Skill;

namespace Application.Core.Game.Maps.AnimatedObjects;



/**
 * @author Jan
 */
public class Summon : AbstractAnimatedMapObject, ICombatantObject
{
    private Player owner;
    private int hp;
    public SummonMovementType MovementType { get; }
    public SummonAssistantType AssistantType { get; }

    public override Player? Controller => owner;

    public long LastAttackTime { get; set; }
    public StatEffect StatEffect { get; }

    public Summon(Player owner, StatEffect statEffect, Point pos) : base(owner.MapModel, pos, 0)
    {
        this.owner = owner;
        StatEffect = statEffect;

        var template = (StatEffect.SourceTemplate as SkillTemplate)!.SummonNode!;
        MovementType = template.GetSummonMovementType();

        AssistantType = template.GetSummonAssistantType();
    }

    public override Task sendSpawnData(IChannelClient client)
    {
        return client.SendPacket(SummonPackets.SpawnSummon(this, false));
    }

    public override Task sendDestroyData(IChannelClient client)
    {
        return client.SendPacket(SummonPackets.RemoveSummon(this, SummonRemoveType.Normal));
    }

    public Player getOwner()
    {
        return owner;
    }

    public override string GetReadableName(IChannelClient c)
    {
        return base.GetReadableName(c) + $"Owner {owner.GetReadableName(c)}, HP: {hp}, Skill: {getSkill()}, SkillLevel: {getSkillLevel()}";
    }


    public int getSkill()
    {
        return StatEffect.GetSkill()!.getId();
    }

    public int getHP()
    {
        return hp;
    }

    public void addHP(int delta)
    {
        hp += delta;
    }
    public sbyte getSkillLevel()
    {
        return (sbyte)StatEffect.SkillLevel;
    }

    public override MapObjectType getType()
    {
        return MapObjectType.SUMMON;
    }

    public bool isPuppet()
    {
        return getSkill() == Ranger.PUPPET || getSkill() == Sniper.PUPPET || getSkill() == WindArcher.PUPPET;
    }

    public override bool IsVisibleForPlayer(Player chr)
    {
        return getOwner() == chr || base.IsVisibleForPlayer(chr) && !chr.HideSummon;
    }

    public override async Task OnMounted(IMap map)
    {
        await base.OnMounted(map);

        if (isPuppet())
        {
            await MapModel.addPlayerPuppet(getOwner());
        }
        setPosition(owner.getPosition());
    }

    public override async Task OnUnmounted()
    {
        if (isPuppet())
        {
            await MapModel.removePlayerPuppet(getOwner());
        }
        await base.OnUnmounted();
    }

    public async Task<bool> DamageBy(ICombatantObject? attacker, int damageValue, short delay, bool stayAlive = false)
    {
        if (!isPuppet())
        {
            // 仅替身术能被造成伤害
            return false;
        }

        if (hp <= 0)
        {
            return false;
        }

        hp -= damageValue;

        if (hp <= 0)
        {
            await MapModel.RemoveMapObject(this, chr => chr.SendPacket(SummonPackets.RemoveSummon(this, SummonRemoveType.Dead)));
            await owner.cancelEffectFromBuffStat(BuffStat.PUPPET);
        }

        return true;
    }
}
