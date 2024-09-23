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

namespace client.command.commands.gm2;

public class SummonCommand : Command
{
    public SummonCommand()
    {
        setDescription("Move a player to your location.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !warphere <playername>");
            return;
        }

        var victim = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
        if (victim == null)
        {
            //If victim isn't on current channel, loop all channels on current world.

            foreach (var ch in Server.getInstance().getChannelsFromWorld(c.getWorld()))
            {
                victim = ch.getPlayerStorage().getCharacterByName(paramsValue[0]);
                if (victim != null)
                {
                    break;//We found the person, no need to continue the loop.
                }
            }
        }
        if (victim != null)
        {
            if (!victim.isLoggedinWorld())
            {
                player.dropMessage(6, "Player currently not logged in or unreachable.");
                return;
            }

            if (player.getClient().getChannel() != victim.getClient().getChannel())
            {//And then change channel if needed.
                victim.dropMessage("Changing channel, please wait a moment.");
                victim.getClient().changeChannel(player.getClient().getChannel());
            }

            try
            {
                for (int i = 0; i < 7; i++)
                {   // poll for a while until the player reconnects
                    if (victim.isLoggedinWorld())
                    {
                        break;
                    }
                    Thread.Sleep(1777);
                }
            }
            catch (ThreadInterruptedException e)
            {
                log.Error(e.ToString());
            }

            var map = player.getMap();
            victim.saveLocationOnWarp();
            victim.forceChangeMap(map, map.findClosestPortal(player.getPosition()));
        }
        else
        {
            player.dropMessage(6, "Unknown player.");
        }
    }
}
