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


using constants.inventory;
using server;

namespace client.command.commands.gm3;

public class FaceCommand : Command
{
    public FaceCommand()
    {
        setDescription("Change face of a player.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !face [<playername>] <faceid>");
            return;
        }

        try
        {
            if (paramsValue.Length == 1)
            {
                int itemId = int.Parse(paramsValue[0]);
                if (!ItemConstants.isFace(itemId) || ItemInformationProvider.getInstance().getName(itemId) == null)
                {
                    player.yellowMessage("Face id '" + paramsValue[0] + "' does not exist.");
                    return;
                }

                player.setFace(itemId);
                player.updateSingleStat(Stat.FACE, itemId);
                player.equipChanged();
            }
            else
            {
                int itemId = int.Parse(paramsValue[1]);
                if (!ItemConstants.isFace(itemId) || ItemInformationProvider.getInstance().getName(itemId) == null)
                {
                    player.yellowMessage("Face id '" + paramsValue[1] + "' does not exist.");
                }

                var victim = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
                if (victim != null)
                {
                    victim.setFace(itemId);
                    victim.updateSingleStat(Stat.FACE, itemId);
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
