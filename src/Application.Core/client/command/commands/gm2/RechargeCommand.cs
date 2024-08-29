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


using client.inventory;
using constants.inventory;
using server;

namespace client.command.commands.gm2;

public class RechargeCommand : Command
{
    public RechargeCommand()
    {
        setDescription("Recharge and refill all USE items.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        foreach (Item torecharge in c.OnlinedCharacter.getInventory(InventoryType.USE).list())
        {
            if (ItemConstants.isThrowingStar(torecharge.getItemId()))
            {
                torecharge.setQuantity(ii.getSlotMax(c, torecharge.getItemId()));
                c.OnlinedCharacter.forceUpdateItem(torecharge);
            }
            else if (ItemConstants.isArrow(torecharge.getItemId()))
            {
                torecharge.setQuantity(ii.getSlotMax(c, torecharge.getItemId()));
                c.OnlinedCharacter.forceUpdateItem(torecharge);
            }
            else if (ItemConstants.isBullet(torecharge.getItemId()))
            {
                torecharge.setQuantity(ii.getSlotMax(c, torecharge.getItemId()));
                c.OnlinedCharacter.forceUpdateItem(torecharge);
            }
            else if (ItemConstants.isConsumable(torecharge.getItemId()))
            {
                torecharge.setQuantity(ii.getSlotMax(c, torecharge.getItemId()));
                c.OnlinedCharacter.forceUpdateItem(torecharge);
            }
        }
        player.dropMessage(5, "USE Recharged.");
    }
}
