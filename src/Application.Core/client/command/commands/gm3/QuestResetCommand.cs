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


using server.quest;

namespace client.command.commands.gm3;

public class QuestResetCommand : Command
{
    public QuestResetCommand()
    {
        setDescription("Reset a completed quest.");
    }

    public override void execute(Client c, string[] paramsValue)
    {
        Character player = c.getPlayer();

        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !resetquest <questid>");
            return;
        }

        int questid_ = int.Parse(paramsValue[0]);

        if (player.getQuestStatus(questid_) != 0)
        {
            Quest quest = Quest.getInstance(questid_);
            if (quest != null)
            {
                quest.reset(player);
                player.dropMessage(5, "QUEST " + questid_ + " reseted.");
            }
            else
            {    // should not occur
                player.dropMessage(5, "QUESTID " + questid_ + " is invalid.");
            }
        }
    }
}
