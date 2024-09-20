/*
    This file is part of the HeavenMS MapleStory NewServer, commands OdinMS-based
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

namespace client.command.commands.gm0;

public class LeaveEventCommand : Command
{
    public LeaveEventCommand()
    {
        setDescription("Leave active event.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        int returnMap = player.getSavedLocation("EVENT");
        if (returnMap != -1)
        {
            if (player.Ola != null)
            {
                player.Ola.resetTimes();
                player.Ola = null;
            }
            if (player.Fitness != null)
            {
                player.Fitness.resetTimes();
                player.Fitness = null;
            }

            player.saveLocationOnWarp();
            player.changeMap(returnMap);
            if (c.getChannelServer().getEvent() != null)
            {
                c.getChannelServer().getEvent().addLimit();
            }
        }
        else
        {
            player.dropMessage(5, "You are not currently in an event.");
        }

    }
}
