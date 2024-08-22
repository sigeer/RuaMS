/*
    This file is part of the HeavenMS MapleStory Server, commands OdinMS-based
    Copyleft (L) 2016 - 2019 RonanLana

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

/*
   @Author: Arthur L - Refactored command content into modules
*/


using server.life;
using server.maps;

namespace client.command.commands.gm3;

public class DebuffCommand : Command
{
    public DebuffCommand()
    {
        setDescription("Put a debuff on all nearby players.");
    }

    public override void execute(Client c, string[] paramsValue)
    {
        Character player = c.getPlayer();
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !debuff SLOW|SEDUCE|ZOMBIFY|CONFUSE|STUN|POISON|SEAL|DARKNESS|WEAKEN|CURSE");
            return;
        }

        Disease? disease = null;
        MobSkill? skill = null;

        switch (paramsValue[0].ToUpper())
        {
            case "SLOW":
                {
                    disease = Disease.SLOW;
                    skill = MobSkillFactory.getMobSkill(MobSkillType.SLOW, 7);
                    break;
                }
            case "SEDUCE":
                {
                    disease = Disease.SEDUCE;
                    skill = MobSkillFactory.getMobSkill(MobSkillType.SEDUCE, 7);
                    break;
                }
            case "ZOMBIFY":
                {
                    disease = Disease.ZOMBIFY;
                    skill = MobSkillFactory.getMobSkill(MobSkillType.UNDEAD, 1); break;
                }
            case "CONFUSE":
                {
                    disease = Disease.CONFUSE;
                    skill = MobSkillFactory.getMobSkill(MobSkillType.REVERSE_INPUT, 2); break;
                }
            case "STUN":
                {
                    disease = Disease.STUN;
                    skill = MobSkillFactory.getMobSkill(MobSkillType.STUN, 7); break;
                }
            case "POISON":
                {
                    disease = Disease.POISON;
                    skill = MobSkillFactory.getMobSkill(MobSkillType.POISON, 5); break;
                }
            case "SEAL":
                {
                    disease = Disease.SEAL;
                    skill = MobSkillFactory.getMobSkill(MobSkillType.SEAL, 1); break;
                }
            case "DARKNESS":
                {
                    disease = Disease.DARKNESS;
                    skill = MobSkillFactory.getMobSkill(MobSkillType.DARKNESS, 1); break;
                }
            case "WEAKEN":
                {
                    disease = Disease.WEAKEN;
                    skill = MobSkillFactory.getMobSkill(MobSkillType.WEAKNESS, 1); break;
                }
            case "CURSE":
                {
                    disease = Disease.CURSE;
                    skill = MobSkillFactory.getMobSkill(MobSkillType.CURSE, 1); break;
                }
        }

        if (disease == null || skill == null)
        {
            player.yellowMessage("Syntax: !debuff SLOW|SEDUCE|ZOMBIFY|CONFUSE|STUN|POISON|SEAL|DARKNESS|WEAKEN|CURSE");
            return;
        }

        foreach (MapObject mmo in player.getMap().getMapObjectsInRange(player.getPosition(), 777777.7, Arrays.asList(MapObjectType.PLAYER)))
        {
            Character chr = (Character)mmo;

            if (chr.getId() != player.getId())
            {
                chr.giveDebuff(disease, skill);
            }
        }
    }
}
