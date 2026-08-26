/*
    This file is part of the HeavenMS MapleStory Server
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

namespace client.inventory.manipulator;


/// <summary>
/// @author RonanLana
/// </summary>
public class KarmaManipulator
{
    private static ItemFlag getKarmaFlag(Item item)
    {
        return item.getItemType() == 1 ? ItemFlag.KARMA_EQP : ItemFlag.KARMA_USE;
    }

    public static bool hasKarmaFlag(Item item)
    {
        ItemFlag karmaFlag = getKarmaFlag(item);
        return item.HasFlag(karmaFlag);
    }

    public static void toggleKarmaFlagToUntradeable(Item item)
    {
        ItemFlag karmaFlag = getKarmaFlag(item);

        item.AddFlag(ItemFlag.UNTRADEABLE);
        item.RemoveFlag(karmaFlag);
    }

    public static void setKarmaFlag(Item item)
    {
        ItemFlag karmaFlag = getKarmaFlag(item);

        item.AddFlag(karmaFlag);
        item.RemoveFlag(ItemFlag.UNTRADEABLE);
    }
}
