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
using client.inventory.manipulator;

namespace client.command.commands.gm2;

public class ClearSlotCommand : Command
{
    public ClearSlotCommand()
    {
        setDescription("Clear all items in an inventory tab.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !clearslot <all, equip, use, setup, etc or cash.>");
            return;
        }
        string type = paramsValue[0];
        switch (type)
        {
            case "all":
                for (int i = 0; i < 101; i++)
                {
                    var tempItem = c.OnlinedCharacter.getInventory(InventoryType.EQUIP).getItem((byte)i);
                    if (tempItem == null)
                    {
                        continue;
                    }
                    InventoryManipulator.removeFromSlot(c, InventoryType.EQUIP, (byte)i, tempItem.getQuantity(), false, false);
                }
                for (int i = 0; i < 101; i++)
                {
                    var tempItem = c.OnlinedCharacter.getInventory(InventoryType.USE).getItem((byte)i);
                    if (tempItem == null)
                    {
                        continue;
                    }
                    InventoryManipulator.removeFromSlot(c, InventoryType.USE, (byte)i, tempItem.getQuantity(), false, false);
                }
                for (int i = 0; i < 101; i++)
                {
                    var tempItem = c.OnlinedCharacter.getInventory(InventoryType.ETC).getItem((byte)i);
                    if (tempItem == null)
                    {
                        continue;
                    }
                    InventoryManipulator.removeFromSlot(c, InventoryType.ETC, (byte)i, tempItem.getQuantity(), false, false);
                }
                for (int i = 0; i < 101; i++)
                {
                    var tempItem = c.OnlinedCharacter.getInventory(InventoryType.SETUP).getItem((byte)i);
                    if (tempItem == null)
                    {
                        continue;
                    }
                    InventoryManipulator.removeFromSlot(c, InventoryType.SETUP, (byte)i, tempItem.getQuantity(), false, false);
                }
                for (int i = 0; i < 101; i++)
                {
                    var tempItem = c.OnlinedCharacter.getInventory(InventoryType.CASH).getItem((byte)i);
                    if (tempItem == null)
                    {
                        continue;
                    }
                    InventoryManipulator.removeFromSlot(c, InventoryType.CASH, (byte)i, tempItem.getQuantity(), false, false);
                }
                player.yellowMessage("All Slots Cleared.");
                break;
            case "equip":
                for (int i = 0; i < 101; i++)
                {
                    var tempItem = c.OnlinedCharacter.getInventory(InventoryType.EQUIP).getItem((byte)i);
                    if (tempItem == null)
                    {
                        continue;
                    }
                    InventoryManipulator.removeFromSlot(c, InventoryType.EQUIP, (byte)i, tempItem.getQuantity(), false, false);
                }
                player.yellowMessage("Equipment Slot Cleared.");
                break;
            case "use":
                for (int i = 0; i < 101; i++)
                {
                    var tempItem = c.OnlinedCharacter.getInventory(InventoryType.USE).getItem((byte)i);
                    if (tempItem == null)
                    {
                        continue;
                    }
                    InventoryManipulator.removeFromSlot(c, InventoryType.USE, (byte)i, tempItem.getQuantity(), false, false);
                }
                player.yellowMessage("Use Slot Cleared.");
                break;
            case "setup":
                for (int i = 0; i < 101; i++)
                {
                    var tempItem = c.OnlinedCharacter.getInventory(InventoryType.SETUP).getItem((byte)i);
                    if (tempItem == null)
                    {
                        continue;
                    }
                    InventoryManipulator.removeFromSlot(c, InventoryType.SETUP, (byte)i, tempItem.getQuantity(), false, false);
                }
                player.yellowMessage("Set-Up Slot Cleared.");
                break;
            case "etc":
                for (int i = 0; i < 101; i++)
                {
                    var tempItem = c.OnlinedCharacter.getInventory(InventoryType.ETC).getItem((byte)i);
                    if (tempItem == null)
                    {
                        continue;
                    }
                    InventoryManipulator.removeFromSlot(c, InventoryType.ETC, (byte)i, tempItem.getQuantity(), false, false);
                }
                player.yellowMessage("ETC Slot Cleared.");
                break;
            case "cash":
                for (int i = 0; i < 101; i++)
                {
                    var tempItem = c.OnlinedCharacter.getInventory(InventoryType.CASH).getItem((byte)i);
                    if (tempItem == null)
                    {
                        continue;
                    }
                    InventoryManipulator.removeFromSlot(c, InventoryType.CASH, (byte)i, tempItem.getQuantity(), false, false);
                }
                player.yellowMessage("Cash Slot Cleared.");
                break;
            default:
                player.yellowMessage("Slot" + type + " does not exist!");
                break;
        }
    }
}
