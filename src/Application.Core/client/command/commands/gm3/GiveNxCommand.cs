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

namespace client.command.commands.gm3;

public class GiveNxCommand : Command
{
    public GiveNxCommand()
    {
        setDescription("Give NX to a player.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !givenx [nx, mp, np] [<playername>] <gainnx>");
            return;
        }

        string recv, typeStr = "nx";
        int value, type = 1;
        if (paramsValue.Length > 1)
        {
            if (paramsValue[0].Length == 2)
            {
                switch (paramsValue[0])
                {
                    case "mp":  // maplePoint
                        type = 2;
                        break;
                    case "np":  // nxPrepaid
                        type = 4;
                        break;
                    default:
                        type = 1;
                        break;
                }
                typeStr = paramsValue[0];

                if (paramsValue.Length > 2)
                {
                    recv = paramsValue[1];
                    value = int.Parse(paramsValue[2]);
                }
                else
                {
                    recv = c.OnlinedCharacter.getName();
                    value = int.Parse(paramsValue[1]);
                }
            }
            else
            {
                recv = paramsValue[0];
                value = int.Parse(paramsValue[1]);
            }
        }
        else
        {
            recv = c.OnlinedCharacter.getName();
            value = int.Parse(paramsValue[0]);
        }

        var victim = c.getWorldServer().getPlayerStorage().getCharacterByName(recv);
        if (victim != null && victim.IsOnlined)
        {
            victim.getCashShop().gainCash(type, value);
            player.message(typeStr.ToUpper() + " given.");
        }
        else
        {
            player.message("Player '" + recv + "' could not be found.");
        }
    }
}
