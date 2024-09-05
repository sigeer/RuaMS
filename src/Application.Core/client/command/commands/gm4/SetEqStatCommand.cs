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

namespace client.command.commands.gm4;

public class SetEqStatCommand : Command
{
    public SetEqStatCommand()
    {
        setDescription("Set stats of all equips in inventory.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !seteqstat <stat value> [<spdjmp value>]");
            return;
        }

        short newStat = (short)Math.Max(0, int.Parse(paramsValue[0]));
        short newSpdJmp = paramsValue.Length >= 2 ? short.Parse(paramsValue[1]) : (short)0;
        Inventory equip = player.getInventory(InventoryType.EQUIP);

        for (byte i = 1; i <= equip.getSlotLimit(); i++)
        {
            try
            {
                var eq = (Equip?)equip.getItem(i);
                if (eq == null)
                {
                    continue;
                }

                eq.setWdef(newStat);
                eq.setAcc(newStat);
                eq.setAvoid(newStat);
                eq.setJump(newSpdJmp);
                eq.setMatk(newStat);
                eq.setMdef(newStat);
                eq.setHp(newStat);
                eq.setMp(newStat);
                eq.setSpeed(newSpdJmp);
                eq.setWatk(newStat);
                eq.setDex(newStat);
                eq.setInt(newStat);
                eq.setStr(newStat);
                eq.setLuk(newStat);

                short flag = eq.getFlag();
                flag |= ItemConstants.UNTRADEABLE;
                eq.setFlag(flag);

                player.forceUpdateItem(eq);
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
        }
    }
}
