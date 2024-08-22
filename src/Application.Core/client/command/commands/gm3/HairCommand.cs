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


using constants.inventory;
using server;

namespace client.command.commands.gm3;

public class HairCommand : Command
{
    public HairCommand()
    {
        setDescription("Change hair of a player.");
    }

    public override void execute(Client c, string[] paramsValue)
    {
        Character player = c.getPlayer();
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !hair [<playername>] <hairid>");
            return;
        }

        try
        {
            if (paramsValue.Length == 1)
            {
                int itemId = int.Parse(paramsValue[0]);
                if (!ItemConstants.isHair(itemId) || ItemInformationProvider.getInstance().getName(itemId) == null)
                {
                    player.yellowMessage("Hair id '" + paramsValue[0] + "' does not exist.");
                    return;
                }

                player.setHair(itemId);
                player.updateSingleStat(Stat.HAIR, itemId);
                player.equipChanged();
            }
            else
            {
                int itemId = int.Parse(paramsValue[1]);
                if (!ItemConstants.isHair(itemId) || ItemInformationProvider.getInstance().getName(itemId) == null)
                {
                    player.yellowMessage("Hair id '" + paramsValue[1] + "' does not exist.");
                    return;
                }

                var victim = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
                if (victim != null)
                {
                    victim.setHair(itemId);
                    victim.updateSingleStat(Stat.HAIR, itemId);
                    victim.equipChanged();
                }
                else
                {
                    player.yellowMessage("Player '" + paramsValue[0] + "' has not been found on this channel.");
                }
            }
        }
        catch (Exception e)
        {
        }
    }
}
