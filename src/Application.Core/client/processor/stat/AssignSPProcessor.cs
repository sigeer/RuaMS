/*
	This file is part of the OdinMS Maple Story Server
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
		       Matthias Butz <matze@odinms.de>
		       Jan Christian Meyer <vimes@odinms.de>

    Copyleft (L) 2016 - 2019 RonanLana (HeavenMS)

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


using Application.Core.Game.Skills;
using client.autoban;
using constants.game;
using tools;

namespace client.processor.stat;

/**
 * @author RonanLana - synchronization of SP transaction modules
 */
public class AssignSPProcessor
{
    public static bool CanAssinSP(IPlayer player, int skillid)
    {
        if (skillid == Aran.HIDDEN_FULL_DOUBLE || skillid == Aran.HIDDEN_FULL_TRIPLE || skillid == Aran.HIDDEN_OVER_DOUBLE || skillid == Aran.HIDDEN_OVER_TRIPLE)
        {
            player.sendPacket(PacketCreator.enableActions());
            return false;
        }

        if ((!GameConstants.isPqSkillMap(player.getMapId()) && GameConstants.isPqSkill(skillid))
            || (!player.isGM() && GameConstants.isGMSkills(skillid))
            || (!GameConstants.isInJobTree(skillid, player.getJob().getId()) && !player.isGM()))
        {
            AutobanFactory.PACKET_EDIT.alert(player, "tried to packet edit in distributing sp.");
            player.Log.Warning("Chr {CharacterName} tried to use skill {SkillId} without it being in their job.", player.getName(), skillid);

            player.Client.Disconnect(true, false);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 从<paramref name="SPFrom"/>洗点加到<paramref name="SPTo"/>上
    /// </summary>
    /// <param name="player"></param>
    /// <param name="SPTo"></param>
    /// <param name="SPFrom"></param>
    public static void ResetSkill(IPlayer player, int SPTo, int SPFrom)
    {
        var skillSPTo = SkillFactory.GetSkillTrust(SPTo);
        var skillSPFrom = SkillFactory.GetSkillTrust(SPFrom);

        if (!CanAssinSP(player, SPTo))
        {
            return;
        }

        var curLevel = player.getSkillLevel(skillSPTo);
        var curLevelSPFrom = player.getSkillLevel(skillSPFrom);
        if (curLevel < skillSPTo.getMaxLevel() && curLevelSPFrom > 0)
        {
            player.changeSkillLevel(skillSPFrom, (sbyte)(curLevelSPFrom - 1), player.getMasterLevel(skillSPFrom), -1);
            player.changeSkillLevel(skillSPTo, (sbyte)(curLevel + 1), player.getMasterLevel(skillSPTo), -1);

            // update macros, thanks to Arnah
            if ((curLevelSPFrom - 1) == 0)
            {
                bool updated = false;
                foreach (var macro in player.SkillMacros)
                {
                    if (macro == null)
                    {
                        continue;
                    }

                    bool update = false;// cleaner?
                    if (macro.Skill1 == SPFrom)
                    {
                        update = true;
                        macro.Skill1 = 0;
                    }
                    if (macro.Skill2 == SPFrom)
                    {
                        update = true;
                        macro.Skill2 = 0;
                    }
                    if (macro.Skill3 == SPFrom)
                    {
                        update = true;
                        macro.Skill3 = 0;
                    }
                    if (update)
                    {
                        updated = true;
                        player.updateMacros(macro.Position, macro);
                    }
                }
                if (updated)
                {
                    player.sendMacros();
                }
            }
        }
    }
    public static void SPAssignAction(IChannelClient c, int skillid)
    {
        c.lockClient();
        try
        {
            if (!CanAssinSP(c.OnlinedCharacter, skillid))
            {
                return;
            }

            var player = c.OnlinedCharacter;
            int remainingSp = player.getRemainingSps()[GameConstants.getSkillBook(skillid / 10000)];
            bool isBeginnerSkill = false;

            if (skillid % 10000000 > 999 && skillid % 10000000 < 1003)
            {
                int total = 0;
                for (int i = 0; i < 3; i++)
                {
                    total += player.getSkillLevel(SkillFactory.getSkill(player.getJobType() * 10000000 + 1000 + i));
                }
                remainingSp = Math.Min((player.getLevel() - 1), 6) - total;
                isBeginnerSkill = true;
            }
            var skill = SkillFactory.GetSkillTrust(skillid);
            int curLevel = player.getSkillLevel(skill);
            if ((remainingSp > 0 && curLevel + 1 <= (skill.isFourthJob() ? player.getMasterLevel(skill) : skill.getMaxLevel())))
            {
                if (!isBeginnerSkill)
                {
                    player.gainSp(-1, GameConstants.getSkillBook(skillid / 10000), false);
                }
                else
                {
                    player.sendPacket(PacketCreator.enableActions());
                }
                if (skill.getId() == Aran.FULL_SWING)
                {
                    player.changeSkillLevel(skill, (sbyte)(curLevel + 1), player.getMasterLevel(skill), player.getSkillExpiration(skill));
                    player.changeSkillLevel(SkillFactory.GetSkillTrust(Aran.HIDDEN_FULL_DOUBLE), player.getSkillLevel(skill), player.getMasterLevel(skill), player.getSkillExpiration(skill));
                    player.changeSkillLevel(SkillFactory.GetSkillTrust(Aran.HIDDEN_FULL_TRIPLE), player.getSkillLevel(skill), player.getMasterLevel(skill), player.getSkillExpiration(skill));
                }
                else if (skill.getId() == Aran.OVER_SWING)
                {
                    player.changeSkillLevel(skill, (sbyte)(curLevel + 1), player.getMasterLevel(skill), player.getSkillExpiration(skill));
                    player.changeSkillLevel(SkillFactory.GetSkillTrust(Aran.HIDDEN_OVER_DOUBLE), player.getSkillLevel(skill), player.getMasterLevel(skill), player.getSkillExpiration(skill));
                    player.changeSkillLevel(SkillFactory.GetSkillTrust(Aran.HIDDEN_OVER_TRIPLE), player.getSkillLevel(skill), player.getMasterLevel(skill), player.getSkillExpiration(skill));
                }
                else
                {
                    player.changeSkillLevel(skill, (sbyte)(curLevel + 1), player.getMasterLevel(skill), player.getSkillExpiration(skill));
                }
            }
        }
        finally
        {
            c.unlockClient();
        }
    }
}
