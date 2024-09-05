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
   @Author: MedicOP - Add clock commands
*/


using tools;

namespace client.command.commands.gm3;

public class TimerAllCommand : Command
{
    public TimerAllCommand()
    {
        setDescription("Set a server wide timer.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !timerall <seconds>|remove");
            return;
        }

        if (paramsValue[0].Equals("remove", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var victim in player.getWorldServer().getPlayerStorage().getAllCharacters())
            {
                victim.sendPacket(PacketCreator.removeClock());
            }
        }
        else
        {
            try
            {
                int seconds = int.Parse(paramsValue[0]);
                foreach (var victim in player.getWorldServer().getPlayerStorage().getAllCharacters())
                {
                    victim.sendPacket(PacketCreator.getClock(seconds));
                }
            }
            catch (FormatException e)
            {
                player.yellowMessage("Syntax: !timerall <seconds>|remove");
            }
        }
    }
}
