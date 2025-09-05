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

using static Application.Templates.Quest.QuestDemand;

namespace server.quest.requirements;


/**
 * @author Tyler (Twdtwd)
 */
public class InfoExRequirement : AbstractQuestRequirement
{
    private List<string> infoExpected = new();
    private int questID;


    public InfoExRequirement(Quest quest, QuestInfoEx[] data) : base(QuestRequirementType.INFO_EX)
    {
        questID = quest.getId();
        infoExpected.AddRange(data.Select(x => x.Value));
    }


    public override bool check(IPlayer chr, int? npcid)
    {
        return true;
    }

    public List<string> getInfo()
    {
        return infoExpected;
    }
}
