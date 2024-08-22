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
public enum QuestRequirementType
{
    UNDEFINED = -1, JOB = 0, ITEM = 1, QUEST = 2, MIN_LEVEL = 3, MAX_LEVEL = 4, END_DATE = 5, MOB = 6, NPC = 7, FIELD_ENTER = 8, INTERVAL = 9, SCRIPT = 10, PET = 11, MIN_PET_TAMENESS = 12, MONSTER_BOOK = 13, NORMAL_AUTO_START = 14, INFO_NUMBER = 15, INFO_EX = 16, COMPLETED_QUEST = 17, START = 18, END = 19, DAY_BY_DAY = 20, MESO = 21, BUFF = 22, EXCEPT_BUFF = 23

}
public static class QuestRequirementTypeUtils
{

    public static sbyte getType(this QuestRequirementType type)
    {
        return (sbyte)type;
    }

    public static QuestRequirementType getByWZName(string name)
    {
        if (name.Equals("job"))
        {
            return QuestRequirementType.JOB;
        }
        else if (name.Equals("quest"))
        {
            return QuestRequirementType.QUEST;
        }
        else if (name.Equals("item"))
        {
            return QuestRequirementType.ITEM;
        }
        else if (name.Equals("lvmin"))
        {
            return QuestRequirementType.MIN_LEVEL;
        }
        else if (name.Equals("lvmax"))
        {
            return QuestRequirementType.MAX_LEVEL;
        }
        else if (name.Equals("end"))
        {
            return QuestRequirementType.END_DATE;
        }
        else if (name.Equals("mob"))
        {
            return QuestRequirementType.MOB;
        }
        else if (name.Equals("npc"))
        {
            return QuestRequirementType.NPC;
        }
        else if (name.Equals("fieldEnter"))
        {
            return QuestRequirementType.FIELD_ENTER;
        }
        else if (name.Equals("interval"))
        {
            return QuestRequirementType.INTERVAL;
        }
        else if (name.Equals("startscript"))
        {
            return QuestRequirementType.SCRIPT;
        }
        else if (name.Equals("endscript"))
        {
            return QuestRequirementType.SCRIPT;
        }
        else if (name.Equals("pet"))
        {
            return QuestRequirementType.PET;
        }
        else if (name.Equals("pettamenessmin"))
        {
            return QuestRequirementType.MIN_PET_TAMENESS;
        }
        else if (name.Equals("mbmin"))
        {
            return QuestRequirementType.MONSTER_BOOK;
        }
        else if (name.Equals("normalAutoStart"))
        {
            return QuestRequirementType.NORMAL_AUTO_START;
        }
        else if (name.Equals("infoNumber"))
        {
            return QuestRequirementType.INFO_NUMBER;
        }
        else if (name.Equals("infoex"))
        {
            return QuestRequirementType.INFO_EX;
        }
        else if (name.Equals("questComplete"))
        {
            return QuestRequirementType.COMPLETED_QUEST;
        }
        else if (name.Equals("start"))
        {
            return QuestRequirementType.START;
            /*} else if(name.Equals("end")) {   already coded
                    return END;*/
        }
        else if (name.Equals("daybyday"))
        {
            return QuestRequirementType.DAY_BY_DAY;
        }
        else if (name.Equals("money"))
        {
            return QuestRequirementType.MESO;
        }
        else if (name.Equals("buff"))
        {
            return QuestRequirementType.BUFF;
        }
        else if (name.Equals("exceptbuff"))
        {
            return QuestRequirementType.EXCEPT_BUFF;
        }
        else
        {
            return QuestRequirementType.UNDEFINED;
        }
    }
}
