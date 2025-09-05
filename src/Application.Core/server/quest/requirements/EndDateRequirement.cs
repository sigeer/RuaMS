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

using System.Globalization;

namespace server.quest.requirements;



/**
 * @author Tyler (Twdtwd)
 */
public class EndDateRequirement : AbstractQuestRequirement
{
    private DateTimeOffset timeStr;


    public EndDateRequirement(Quest quest, string dateString) : base(QuestRequirementType.END_DATE)
    {
        DateTime dateTime = DateTime.ParseExact(
            dateString,
            "yyyyMMddHH",
            CultureInfo.InvariantCulture
        );

        timeStr = new DateTimeOffset(dateTime, TimeSpan.Zero);
    }
    public override bool check(IPlayer chr, int? npcid)
    {
        return timeStr >= chr.Client.CurrentServerContainer.GetCurrentTimeDateTimeOffSet();
    }
}
