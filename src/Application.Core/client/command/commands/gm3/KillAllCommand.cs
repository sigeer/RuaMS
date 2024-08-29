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


using Application.Core.Game.Life;
using constants.id;
using server.maps;

namespace client.command.commands.gm3;




public class KillAllCommand : Command
{
    public KillAllCommand()
    {
        setDescription("Kill all mobs in the map.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        var map = player.getMap();
        var monsters = map.getMapObjectsInRange(player.getPosition(), double.PositiveInfinity, Arrays.asList(MapObjectType.MONSTER));
        int count = 0;
        foreach (var monstermo in monsters)
        {
            Monster monster = (Monster)monstermo;
            if (!monster.getStats().isFriendly() && !(monster.getId() >= MobId.DEAD_HORNTAIL_MIN && monster.getId() <= MobId.HORNTAIL))
            {
                map.damageMonster(player, monster, int.MaxValue);
                count++;
            }
        }
        player.dropMessage(5, "Killed " + count + " monsters.");
    }
}
