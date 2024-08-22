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

namespace client.command.commands.gm2;

public class ApCommand : Command
{
    public ApCommand()
    {
        setDescription("Set available AP.");
    }

    public override void execute(Client c, string[] paramsValue)
    {
        Character player = c.getPlayer();
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !ap [<playername>] <newap>");
            return;
        }

        if (paramsValue.Length < 2)
        {
            int newAp = int.Parse(paramsValue[0]);
            if (newAp < 0)
            {
                newAp = 0;
            }
            else if (newAp > YamlConfig.config.server.MAX_AP)
            {
                newAp = YamlConfig.config.server.MAX_AP;
            }

            player.changeRemainingAp(newAp, false);
        }
        else
        {
            var victim = c.getWorldServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
            if (victim != null)
            {
                int newAp = int.Parse(paramsValue[1]);
                if (newAp < 0)
                {
                    newAp = 0;
                }
                else if (newAp > YamlConfig.config.server.MAX_AP)
                {
                    newAp = YamlConfig.config.server.MAX_AP;
                }

                victim.changeRemainingAp(newAp, false);
            }
            else
            {
                player.message("Player '" + paramsValue[0] + "' could not be found.");
            }
        }
    }
}
