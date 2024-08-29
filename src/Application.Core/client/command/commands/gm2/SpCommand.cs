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

namespace client.command.commands.gm2;

public class SpCommand : Command
{
    public SpCommand()
    {
        setDescription("Set available SP.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !sp [<playername>] <newsp>");
            return;
        }

        if (paramsValue.Length == 1)
        {
            int newSp = int.Parse(paramsValue[0]);
            if (newSp < 0)
            {
                newSp = 0;
            }
            else if (newSp > YamlConfig.config.server.MAX_AP)
            {
                newSp = YamlConfig.config.server.MAX_AP;
            }

            player.updateRemainingSp(newSp);
        }
        else
        {
            var victim = c.getWorldServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
            if (victim != null && victim.IsOnlined)
            {
                int newSp = int.Parse(paramsValue[1]);
                if (newSp < 0)
                {
                    newSp = 0;
                }
                else if (newSp > YamlConfig.config.server.MAX_AP)
                {
                    newSp = YamlConfig.config.server.MAX_AP;
                }

                victim.updateRemainingSp(newSp);

                player.dropMessage(5, "SP given.");
            }
            else
            {
                player.message("Player '" + paramsValue[0] + "' could not be found.");
            }
        }
    }
}
