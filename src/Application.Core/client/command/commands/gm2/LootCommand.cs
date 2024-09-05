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
   @Author: Resinate
*/


using Application.Core.Game.Maps;
using server.maps;

namespace client.command.commands.gm2;

public class LootCommand : Command
{
    public LootCommand()
    {
        setDescription("Loots all items that belong to you.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var items = c.OnlinedCharacter.getMap().getMapObjectsInRange(c.OnlinedCharacter.getPosition(), double.PositiveInfinity, Arrays.asList(MapObjectType.ITEM));
        foreach (var item in items)
        {
            MapItem mapItem = (MapItem)item;
            if (mapItem.getOwnerId() == c.OnlinedCharacter.getId() || mapItem.getOwnerId() == c.OnlinedCharacter.getPartyId())
            {
                c.OnlinedCharacter.pickupItem(mapItem);
            }
        }

    }
}
