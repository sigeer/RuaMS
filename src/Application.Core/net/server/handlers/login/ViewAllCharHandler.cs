/*
	This file is part of the OdinMS Maple Story NewServer
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
		       Matthias Butz <matze@odinms.de>
		       Jan Christian Meyer <vimes@odinms.de>

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


using net.packet;
using tools;

namespace net.server.handlers.login;

public class ViewAllCharHandler : AbstractPacketHandler
{
    private static int CHARACTER_LIMIT = 60; // IClient will crash if sending 61 or more characters

    public override void HandlePacket(InPacket p, IClient c)
    {
        try
        {
            if (!c.canRequestCharlist())
            {   // client breaks if the charlist request pops too soon
                c.sendPacket(PacketCreator.showAllCharacter(0, 0));
                return;
            }

            var worldChrs = Server.getInstance().loadAccountCharlist(c.getAccID(), c.getVisibleWorlds());
            worldChrs = limitTotalChrs(worldChrs, CHARACTER_LIMIT);

            padChrsIfNeeded(worldChrs);

            int totalWorlds = worldChrs.Count;
            int totalChrs = countTotalChrs(worldChrs);
            c.sendPacket(PacketCreator.showAllCharacter(totalWorlds, totalChrs));

            bool usePic = YamlConfig.config.server.ENABLE_PIC && !c.canBypassPic();
            foreach (var item in worldChrs)
            {
                c.sendPacket(PacketCreator.showAllCharacterInfo(item.Key, item.Value, usePic));
            }
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    private static SortedDictionary<int, List<IPlayer>> limitTotalChrs(SortedDictionary<int, List<IPlayer>> worldChrs, int limit)
    {
        if (countTotalChrs(worldChrs) <= limit)
        {
            return worldChrs;
        }
        else
        {
            ;
            return cutAfterChrLimit(worldChrs, limit);
        }
    }

    private static int countTotalChrs(IDictionary<int, List<IPlayer>> worldChrs)
    {
        return worldChrs.Sum(x => x.Value.Count);
    }

    private static SortedDictionary<int, List<IPlayer>> cutAfterChrLimit(SortedDictionary<int, List<IPlayer>> worldChrs,
                                                                        int limit)
    {
        SortedDictionary<int, List<IPlayer>> cappedCopy = new();
        int runningChrTotal = 0;
        foreach (var entry in worldChrs)
        {
            int worldId = entry.Key;
            List<IPlayer> chrs = entry.Value;
            if (runningChrTotal + chrs.Count <= limit)
            {
                // Limit not reached, move them all
                runningChrTotal += chrs.Count;
                cappedCopy.AddOrUpdate(worldId, chrs);
            }
            else
            { // Limit would be reached if all chrs were moved. Move just enough to fit within limit.
                int remainingSlots = limit - runningChrTotal;
                List<IPlayer> lastChrs = chrs.Take(remainingSlots).ToList();
                cappedCopy.AddOrUpdate(worldId, lastChrs);
                break;
            }
        }

        return cappedCopy;
    }

    /**
     * If there are more characters than fits the screen (9), and you start scrolling down,
     * the characters on the last row will not appear unless the row is completely filled.
     * Meaning, if there are 1 or 2 characters remaining on the last row, they will not appear.
     *
     * @param totalChrs total amount of characters to display on 'View all characters' screen
     * @return if we need to pad the last row to include the characters that would otherwise not appear
     */
    private static void padChrsIfNeeded(SortedDictionary<int, List<IPlayer>> worldChrs)
    {
        while (shouldPadLastRow(countTotalChrs(worldChrs)))
        {
            List<IPlayer> lastWorldChrs = worldChrs.Last().Value;
            var lastChrForPadding = lastWorldChrs.Last();
            lastWorldChrs.Add(lastChrForPadding);
        }
    }

    private static bool shouldPadLastRow(int totalChrs)
    {
        bool shouldScroll = totalChrs > 9;
        bool isLastRowFilled = totalChrs % 3 == 0;
        return shouldScroll && !isLastRowFilled;
    }
}
