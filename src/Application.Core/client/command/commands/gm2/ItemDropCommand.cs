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
using client.inventory;
using constants.inventory;
using server;

namespace client.command.commands.gm2;

public class ItemDropCommand : Command
{
    public ItemDropCommand()
    {
        setDescription("Spawn an item onto the ground.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !drop <itemid> <quantity>");
            return;
        }

        int itemId = int.Parse(paramsValue[0]);
        ItemInformationProvider ii = ItemInformationProvider.getInstance();

        if (ii.getName(itemId) == null)
        {
            player.yellowMessage("Item id '" + paramsValue[0] + "' does not exist.");
            return;
        }

        short quantity = 1;
        if (paramsValue.Length >= 2)
        {
            quantity = short.Parse(paramsValue[1]);
        }

        if (YamlConfig.config.server.BLOCK_GENERATE_CASH_ITEM && ii.isCash(itemId))
        {
            player.yellowMessage("You cannot create a cash item with this command.");
            return;
        }

        if (ItemConstants.isPet(itemId))
        {
            if (paramsValue.Length >= 2)
            {   // thanks to istreety & TacoBell
                quantity = 1;
                long days = Math.Max(1, int.Parse(paramsValue[1]));
                long expiration = DateTimeOffset.Now.AddDays(days).ToUnixTimeMilliseconds();
                int petid = ItemManager.CreatePet(itemId);

                var toDropTemp = new Item(itemId, 0, quantity, petid);
                toDropTemp.setExpiration(expiration);

                toDropTemp.setOwner("");
                if (player.gmLevel() < 3)
                {
                    short f = toDropTemp.getFlag();
                    f |= ItemConstants.ACCOUNT_SHARING;
                    f |= ItemConstants.UNTRADEABLE;
                    f |= ItemConstants.SANDBOX;

                    toDropTemp.setFlag(f);
                    toDropTemp.setOwner("TRIAL-MODE");
                }

                c.OnlinedCharacter.getMap().spawnItemDrop(c.OnlinedCharacter, c.OnlinedCharacter, toDropTemp, c.OnlinedCharacter.getPosition(), true, true);

                return;
            }
            else
            {
                player.yellowMessage("Pet Syntax: !drop <itemid> <expiration>");
                return;
            }
        }

        Item toDrop;
        if (ItemConstants.getInventoryType(itemId) == InventoryType.EQUIP)
        {
            toDrop = ii.getEquipById(itemId);
        }
        else
        {
            toDrop = new Item(itemId, 0, quantity);
        }

        toDrop.setOwner(player.getName());
        if (player.gmLevel() < 3)
        {
            short f = toDrop.getFlag();
            f |= ItemConstants.ACCOUNT_SHARING;
            f |= ItemConstants.UNTRADEABLE;
            f |= ItemConstants.SANDBOX;

            toDrop.setFlag(f);
            toDrop.setOwner("TRIAL-MODE");
        }

        c.OnlinedCharacter.getMap().spawnItemDrop(c.OnlinedCharacter, c.OnlinedCharacter, toDrop, c.OnlinedCharacter.getPosition(), true, true);
    }
}
