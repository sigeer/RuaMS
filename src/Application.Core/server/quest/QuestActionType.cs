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
namespace server.quest;

/**
 * @author Matze
 */
public enum QuestActionType
{
    UNDEFINED = -1,
    EXP = 0,
    ITEM = 1,
    NEXTQUEST = 2,
    MESO = 3,
    QUEST = 4,
    SKILL = 5,
    FAME = 6,
    BUFF = 7,
    PETSKILL = 8,
    YES = 9,
    NO = 10,
    NPC = 11,
    MIN_LEVEL = 12,
    NORMAL_AUTO_START = 13,
    PETTAMENESS = 14,
    PETSPEED = 15,
    INFO = 16,
    /// <summary>
    /// 原版这里也是16，但是C#不支持，标记观察
    /// </summary>
    ZERO = 17
}

public static class QuestActionTypeUtils
{
    public static QuestActionType getByWZName(string? name)
    {
        if (name == "exp")
        {
            return QuestActionType.EXP;
        }
        else if (name == "money")
        {
            return QuestActionType.MESO;
        }
        else if (name == "item")
        {
            return QuestActionType.ITEM;
        }
        else if (name == "skill")
        {
            return QuestActionType.SKILL;
        }
        else if (name == "nextQuest")
        {
            return QuestActionType.NEXTQUEST;
        }
        else if (name == "pop")
        {
            return QuestActionType.FAME;
        }
        else if (name == "buffItemID")
        {
            return QuestActionType.BUFF;
        }
        else if (name == "petskill")
        {
            return QuestActionType.PETSKILL;
        }
        else if (name == "no")
        {
            return QuestActionType.NO;
        }
        else if (name == "yes")
        {
            return QuestActionType.YES;
        }
        else if (name == "npc")
        {
            return QuestActionType.NPC;
        }
        else if (name == "lvmin")
        {
            return QuestActionType.MIN_LEVEL;
        }
        else if (name == "normalAutoStart")
        {
            return QuestActionType.NORMAL_AUTO_START;
        }
        else if (name == "pettameness")
        {
            return QuestActionType.PETTAMENESS;
        }
        else if (name == "petspeed")
        {
            return QuestActionType.PETSPEED;
        }
        else if (name == "info")
        {
            return QuestActionType.INFO;
        }
        else if (name == "0")
        {
            return QuestActionType.ZERO;
        }
        else
        {
            return QuestActionType.UNDEFINED;
        }
    }
}