/*
    This file is part of the HeavenMS MapleStory Server
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

namespace server.quest.requirements;

/**
 * @author Ronan
 */
public class InfoNumberRequirement : AbstractQuestRequirement
{

    private short infoNumber;
    private int questID;

    public InfoNumberRequirement(Quest quest, int data) : base(QuestRequirementType.INFO_NUMBER)
    {
        questID = quest.getId();
        infoNumber = (short)data;
    }


    public override bool check(Player chr, int? npcid)
    {
        return true;
    }

    public short getInfoNumber()
    {
        return infoNumber;
    }
}
