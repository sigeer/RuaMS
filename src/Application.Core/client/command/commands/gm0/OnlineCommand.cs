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


using Application.Core.Managers;
using net.server;

namespace client.command.commands.gm0;

public class OnlineCommand : Command
{
    public OnlineCommand()
    {
        setDescription("Show all online players.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        foreach (var ch in Server.getInstance().getChannelsFromWorld(player.getWorld()))
        {
            player.yellowMessage("Players in Channel " + ch.getId() + ":");
            foreach (var chr in ch.getPlayerStorage().getAllCharacters())
            {
                if (!chr.isGM())
                {
                    player.message(" >> " + CharacterManager.makeMapleReadable(chr.getName()) + " is at " + chr.getMap().getMapName() + ".");
                }
            }
        }
    }
}
