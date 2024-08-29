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


using net.server;
using System.Net;
using tools;

namespace client.command.commands.gm6;




public class WarpWorldCommand : Command
{
    public WarpWorldCommand()
    {
        setDescription("Warp to a different world.");
    }

    public override void execute(IClient c, string[] paramValues)
    {
        var player = c.OnlinedCharacter;
        if (paramValues.Length < 1)
        {
            player.yellowMessage("Syntax: !warpworld <worldid>");
            return;
        }

        Server server = Server.getInstance();
        byte worldb = byte.Parse(paramValues[0]);
        if (worldb <= (server.getWorldsSize() - 1))
        {
            try
            {
                string[] socket = server.getInetSocket(c, worldb, c.getChannel());
                c.getWorldServer().removePlayer(player);
                player.getMap().removePlayer(player);//LOL FORGOT THIS ><
                player.setSessionTransitionState();
                player.setWorld(worldb);
                player.saveCharToDB();//To set the new world :O (true because else 2 player instances are created, one in both worlds)
                c.sendPacket(PacketCreator.getChannelChange(IPAddress.Parse(socket[0]), int.Parse(socket[1])));
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                player.message("Unexpected error when changing worlds, are you sure the world you are trying to warp to has the same amount of channels?");
            }

        }
        else
        {
            player.message("Invalid world; highest number available: " + (server.getWorldsSize() - 1));
        }
    }
}
