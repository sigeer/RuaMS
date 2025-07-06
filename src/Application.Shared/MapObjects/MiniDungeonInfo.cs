/*
	This file is part of the OdinMS Maple Story Server
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


using Application.Shared.Constants.Map;
using Application.Utility;

namespace Application.Shared.MapObjects;

/**
 * @author Alan (SharpAceX)
 */

public class MiniDungeonInfo : EnumClass
{

    //http://bbb.hidden-street.net/search_finder/mini%20dungeon

    public static readonly MiniDungeonInfo CAVE_OF_MUSHROOMS = new(MapId.ANT_TUNNEL_2, MapId.CAVE_OF_MUSHROOMS_BASE, 30);
    public static readonly MiniDungeonInfo GOLEM_CASTLE_RUINS = new(MapId.SLEEPY_DUNGEON_4, MapId.GOLEMS_CASTLE_RUINS_BASE, 34);
    public static readonly MiniDungeonInfo HILL_OF_SANDSTORMS = new(MapId.SAHEL_2, MapId.HILL_OF_SANDSTORMS_BASE, 30);
    public static readonly MiniDungeonInfo HENESYS_PIG_FARM = new(MapId.RAIN_FOREST_EAST_OF_HENESYS, MapId.HENESYS_PIG_FARM_BASE, 30);
    public static readonly MiniDungeonInfo DRAKES_BLUE_CAVE = new(MapId.COLD_CRADLE, MapId.DRAKES_BLUE_CAVE_BASE, 30);
    public static readonly MiniDungeonInfo DRUMMER_BUNNYS_LAIR = new(MapId.EOS_TOWER_76TH_TO_90TH_FLOOR, MapId.DRUMMER_BUNNYS_LAIR_BASE, 30);
    public static readonly MiniDungeonInfo THE_ROUND_TABLE_OF_KENTARUS = new(MapId.BATTLEFIELD_OF_FIRE_AND_WATER, MapId.ROUND_TABLE_OF_KENTAURUS_BASE, 30);
    public static readonly MiniDungeonInfo THE_RESTORING_MEMORY = new(MapId.DRAGON_NEST_LEFT_BEHIND, MapId.RESTORING_MEMORY_BASE, 19);
    public static readonly MiniDungeonInfo NEWT_SECURED_ZONE = new(MapId.DESTROYED_DRAGON_NEST, MapId.NEWT_SECURED_ZONE_BASE, 19);
    public static readonly MiniDungeonInfo PILLAGE_OF_TREASURE_ISLAND = new(MapId.RED_NOSE_PIRATE_DEN_2, MapId.PILLAGE_OF_TREASURE_ISLAND_BASE, 30);
    public static readonly MiniDungeonInfo CRITICAL_ERROR = new(MapId.LAB_AREA_C1, MapId.CRITICAL_ERROR_BASE, 30);
    public static readonly MiniDungeonInfo LONGEST_RIDE_ON_BYEBYE_STATION = new(MapId.FANTASY_THEME_PARK_3, MapId.LONGEST_RIDE_ON_BYEBYE_STATION, 19);

    //http://bbb.hidden-street.net/search_finder/mini%20dungeon

    private int baseId;
    private int dungeonId;
    private int dungeons;

    MiniDungeonInfo(int baseId, int dungeonId, int dungeons)
    {
        this.baseId = baseId;
        this.dungeonId = dungeonId;
        this.dungeons = dungeons;
    }

    public int getBase()
    {
        return baseId;
    }

    public int getDungeonId()
    {
        return dungeonId;
    }

    public int getDungeons()
    {
        return dungeons;
    }

    public static bool isDungeonMap(int map)
    {
        foreach (MiniDungeonInfo dungeon in EnumClassCache<MiniDungeonInfo>.Values)
        {
            if (map >= dungeon.getDungeonId() && map <= dungeon.getDungeonId() + dungeon.getDungeons())
            {
                return true;
            }
        }
        return false;
    }

    public static MiniDungeonInfo getDungeon(int map)
    {
        foreach (MiniDungeonInfo dungeon in EnumClassCache<MiniDungeonInfo>.Values)
        {
            if (map >= dungeon.getDungeonId() && map <= dungeon.getDungeonId() + dungeon.getDungeons())
            {
                return dungeon;
            }
        }
        return null;
    }
}
