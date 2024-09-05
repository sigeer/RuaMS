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
   @Author: Ronan
*/

namespace client.command.commands.gm2;

public class SetSlotCommand : Command
{
    public SetSlotCommand()
    {
        setDescription("Set amount of inventory slots in all tabs.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !setslot <newlevel>");
            return;
        }

        int slots = (int.Parse(paramsValue[0]) / 4) * 4;
        for (int i = 1; i < 5; i++)
        {
            int curSlots = player.getSlots(i);
            if (slots <= -curSlots)
            {
                continue;
            }

            player.gainSlots(i, slots - curSlots, true);
        }

        player.yellowMessage("Slots updated.");
    }
}
