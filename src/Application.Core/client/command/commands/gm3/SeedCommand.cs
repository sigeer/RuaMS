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


using client.inventory;
using constants.id;

namespace client.command.commands.gm3;



public class SeedCommand : Command
{
    public SeedCommand()
    {
        setDescription("Drop all seeds inside Henesys PQ.");
    }

    public override void execute(Client c, string[] paramsValue)
    {
        Character player = c.getPlayer();
        if (player.getMapId() != MapId.HENESYS_PQ)
        {
            player.yellowMessage("This command can only be used in HPQ.");
            return;
        }
        Point[] pos = { new Point(7, -207), new Point(179, -447), new Point(-3, -687), new Point(-357, -687), new Point(-538, -447), new Point(-359, -207) };
        int[] seed = {ItemId.PINK_PRIMROSE_SEED, ItemId.PURPLE_PRIMROSE_SEED, ItemId.GREEN_PRIMROSE_SEED,
                ItemId.BLUE_PRIMROSE_SEED, ItemId.YELLOW_PRIMROSE_SEED, ItemId.BROWN_PRIMROSE_SEED};
        for (int i = 0; i < pos.Length; i++)
        {
            Item item = new Item(seed[i], 0, 1);
            player.getMap().spawnItemDrop(player, player, item, pos[i], false, true);
            try
            {
                Thread.Sleep(100);
            }
            catch (ThreadInterruptedException e)
            {
                log.Error(e.ToString());
            }
        }
    }
}
