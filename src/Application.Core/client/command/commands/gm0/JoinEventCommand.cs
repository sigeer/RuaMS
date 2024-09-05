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


using constants.id;
using server.maps;

namespace client.command.commands.gm0;

public class JoinEventCommand : Command
{
    public JoinEventCommand()
    {
        setDescription("Join active event.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (!FieldLimit.CANNOTMIGRATE.check(player.getMap().getFieldLimit()))
        {
            var evt = c.getChannelServer().getEvent();
            if (evt != null)
            {
                if (evt.getMapId() != player.getMapId())
                {
                    if (evt.getLimit() > 0)
                    {
                        player.saveLocation("EVENT");

                        if (evt.getMapId() == MapId.EVENT_COCONUT_HARVEST || evt.getMapId() == MapId.EVENT_SNOWBALL_ENTRANCE)
                        {
                            player.setTeam(evt.getLimit() % 2);
                        }

                        evt.minusLimit();

                        player.saveLocationOnWarp();
                        player.changeMap(evt.getMapId());
                    }
                    else
                    {
                        player.dropMessage(5, "The limit of players for the event has already been reached.");
                    }
                }
                else
                {
                    player.dropMessage(5, "You are already in the event.");
                }
            }
            else
            {
                player.dropMessage(5, "There is currently no event in progress.");
            }
        }
        else
        {
            player.dropMessage(5, "You are currently in a map where you can't join an event.");
        }
    }
}
