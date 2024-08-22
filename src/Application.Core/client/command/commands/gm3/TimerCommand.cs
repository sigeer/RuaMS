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
   @Author: MedicOP - Add clock commands
*/


using tools;

namespace client.command.commands.gm3;

public class TimerCommand : Command
{
    public TimerCommand()
    {
        setDescription("Set timer on a player in current map.");
    }

    public override void execute(Client c, string[] paramsValue)
    {
        Character player = c.getPlayer();
        if (paramsValue.Length < 2)
        {
            player.yellowMessage("Syntax: !timer <playername> <seconds>|remove");
            return;
        }

        var victim = c.getWorldServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
        if (victim != null)
        {
            if (paramsValue[1].Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                victim.sendPacket(PacketCreator.removeClock());
            }
            else
            {
                try
                {
                    victim.sendPacket(PacketCreator.getClock(int.Parse(paramsValue[1])));
                }
                catch (FormatException e)
                {
                    player.yellowMessage("Syntax: !timer <playername> <seconds>|remove");
                }
            }
        }
        else
        {
            player.message("Player '" + paramsValue[0] + "' could not be found.");
        }
    }
}
