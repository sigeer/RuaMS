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


using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Maps.Specials;
using constants.id;
using scripting.Event;
using server.life;
using server.partyquest;
using System.Text;
using tools;
using XmlWzReader.WzEntity;


namespace server.maps;

public class MapFactory
{
    private static Data nameData;
    private static DataProvider mapSource;

    static MapFactory()
    {
        nameData = DataProviderFactory.getDataProvider(WZFiles.STRING).getData("Map.img");
        mapSource = DataProviderFactory.getDataProvider(WZFiles.MAP);
    }

    private static void loadLifeFromWz(IMap map, MapWzBase mapData)
    {
        foreach (var mapLife in mapData.Lives)
        {
            loadLifeRaw(map, mapLife.LifeId, mapLife.Type, mapLife.Cy, mapLife.F, mapLife.Fh, mapLife.Rx0, mapLife.Rx1, mapLife.X, mapLife.Y, mapLife.Hide, mapLife.MobTime, mapLife.Team);
        }
    }

    private static void loadLifeFromDb(IMap map)
    {
        try
        {
            using var dbContext = new DBContext();
            var dataList = dbContext.Plives.Where(x => x.Map == map.getId() && x.World == map.getWorld()).ToList();

            foreach (var rs in dataList)
            {
                int id = rs.Life;
                string type = rs.Type;
                int cy = rs.Cy;
                int f = rs.F;
                int fh = rs.Fh;
                int rx0 = rs.Rx0;
                int rx1 = rs.Rx1;
                int x = rs.X;
                int y = rs.Y;
                int hide = rs.Hide;
                int mobTime = rs.Mobtime;
                int team = rs.Team;

                loadLifeRaw(map, id, type, cy, f, fh, rx0, rx1, x, y, hide == 1, mobTime, team);
            }

        }
        catch (Exception sqle)
        {
            Log.Logger.Error(sqle.ToString());
        }
    }

    private static void loadLifeRaw(IMap map, int id, string type, int cy, int f, int fh, int rx0, int rx1, int x, int y, bool hide, int mobTime, int team)
    {
        AbstractLifeObject myLife = loadLife(id, type, cy, f, fh, rx0, rx1, x, y, hide);
        if (myLife is Monster monster)
        {

            if (mobTime == -1)
            { //does not respawn, force spawn once
                map.spawnMonster(monster);
            }
            else
            {
                map.addMonsterSpawn(monster, mobTime, team);
            }

            //should the map be reseted, use allMonsterSpawn list of monsters to spawn them again
            map.addAllMonsterSpawn(monster, mobTime, team);
        }
        else
        {
            map.addMapObject(myLife);
        }
    }

    public static IMap loadMapFromWz(int mapid, int world, int channel, EventInstanceManager? evt)
    {
        IMap map;

        string mapName = getMapName(mapid);
        var mapData = mapSource.getData(mapName);    // source.getData issue with giving nulls in rare ocasions found thanks to MedicOP
        if (mapData == null)
            throw new BusinessResException($"Map {mapName} not existed");

        var infoData = mapData.getChildByPath("info");

        string link = DataTool.getString(infoData?.getChildByPath("link")) ?? "";
        if (!string.IsNullOrEmpty(link))
        {
            //nexon made hundreds of dojo maps so to reduce the size they added links.
            mapName = getMapName(int.Parse(link));
            mapData = mapSource.getData(mapName);
        }

        var mapModel = new MapWzBase(mapid, mapData);
        var infoModel = mapModel.MapInfo;

        map = new MapleMap(mapid, world, channel, infoModel.ReturnMap, infoModel.MobRate);

        if (mapModel.CPQInfo != null)
            map = new MonsterCarnivalMap(map);

        map.setEventInstance(evt);

        map.setOnFirstUserEnter(infoModel.OnFirstUserEnter);
        map.setOnUserEnter(infoModel.OnUserEnter);
        map.setFieldLimit(infoModel.FieldLimit);
        map.setMobInterval(infoModel.CreateMobInterval);
        PortalFactory portalFactory = new PortalFactory();
        foreach (var portal in mapModel.Portals)
        {
            map.addPortal(portalFactory.makePortal(portal));
        }

        if (infoModel.TimeMob != null)
        {
            map.setTimeMob(infoModel.TimeMob.Id, infoModel.TimeMob.Message);
        }

        map.SetMapArea(mapModel.MapArea);

        FootholdTree fTree = new FootholdTree(new Point(mapModel.FootholdMinX1, mapModel.FootholdMinY1), new Point(mapModel.FootholdMaxX2, mapModel.FootholdMaxY2));
        var allFootholds = mapModel.Footholds.Select(item =>
        {
            var m = new Foothold(new Point(item.X1, item.Y1), new Point(item.X2, item.Y2), item.Index);
            m.setNext(item.Next);
            m.setPrev(item.Prev);
            return m;
        }).ToList();
        foreach (Foothold fh in allFootholds)
        {
            fTree.insert(fh);
        }
        map.setFootholds(fTree);

        foreach (var area in mapModel.Areas)
        {
            map.addMapleArea(area.Rect);
        }

        map.setSeats(mapModel.SeatCount);

        if (evt == null)
        {
            try
            {
                using var dbContext = new DBContext();
                var dataList = dbContext.Playernpcs.Where(x => x.Map == mapid && x.World == world).ToList();

                foreach (var item in dataList)
                {
                    map.addPlayerNPCMapObject(new PlayerNPC(item));
                }

            }
            catch (Exception e)
            {
                Log.Logger.Error(e.ToString());
            }
        }

        loadLifeFromWz(map, mapModel);
        loadLifeFromDb(map);

        if (map is MonsterCarnivalMap cpqMap)
        {
            cpqMap.DeathCP = mapModel.CPQInfo!.DeathCP;
            cpqMap.MaxMobs = mapModel.CPQInfo!.MaxMobs;    // thanks Atoot for noticing CPQ1 bf. 3 and 4 not accepting spawns due to undefined limits, Lame for noticing a need to cap mob spawns even on such undefined limits
            cpqMap.TimeDefault = mapModel.CPQInfo!.TimeDefault;
            cpqMap.TimeExpand = mapModel.CPQInfo!.TimeExpand;
            cpqMap.MaxReactors = mapModel.CPQInfo!.MaxReactors;

            foreach (var item in mapModel.CPQInfo!.GuardianSpawnPoints)
            {
                GuardianSpawnPoint pt = new GuardianSpawnPoint(item.Point);
                pt.setTeam(item.Team);
                pt.setTaken(false);
                cpqMap.AddGuardianSpawnPoint(pt);
            }

            foreach (var area in mapModel.CPQInfo!.Skills)
            {
                cpqMap.AddSkillId(area);
            }

            foreach (var item in mapModel.CPQInfo!.CPQMobDataList)
            {
                cpqMap.AddMobSpawn(item.Id, item.CP);
            }
        }

        foreach (var item in mapModel.Reactors)
        {
            map.spawnReactor(loadReactor(item));
        }

        map.setMapName(loadPlaceName(mapid));
        map.setStreetName(loadStreetName(mapid));

        map.setClock(mapModel.Clock);
        map.setEverlast(infoModel.Everlast); // thanks davidlafriniere for noticing value 0 accounting as true
        map.setTown(infoModel.Town);
        map.setHPDec(infoModel.DecHP);
        map.setHPDecProtect(infoModel.ProtectItem);
        map.setForcedReturnMap(infoModel.ForceReturn);
        map.setBoat(mapModel.Boat);
        map.setTimeLimit(infoModel.TimeLimit);
        map.setFieldType(infoModel.FieldType);
        map.setMobCapacity(infoModel.FixedMobCapacity);//Is there a map that contains more than 500 mobs?
        map.setRecovery(infoModel.Recovery);
        map.setBackgroundTypes(mapModel.Backs.Select(x => new KeyValuePair<int, int>(x.Index, x.Type)).ToDictionary());
        map.generateMapDropRangeCache();

        return map;
    }

    private static AbstractLifeObject loadLife(int id, string type, int cy, int f, int fh, int rx0, int rx1, int x, int y, bool hide)
    {
        AbstractLifeObject myLife = LifeFactory.getLife(id, type) ?? throw new BusinessResException($"LifeFactory.getLife({id}, {type})");
        myLife.setCy(cy);
        myLife.setF(f);
        myLife.setFh(fh);
        myLife.setRx0(rx0);
        myLife.setRx1(rx1);
        myLife.setPosition(new Point(x, y));
        myLife.setHide(hide);
        return myLife;
    }

    private static Reactor loadReactor(MapReactor mapReactor)
    {
        Reactor myReactor = new Reactor(ReactorFactory.getReactor(mapReactor.ReactorId), mapReactor.ReactorId);
        myReactor.setFacingDirection(mapReactor.FacingDirection);
        myReactor.setPosition(mapReactor.Point);
        myReactor.setDelay(mapReactor.ReactorTime);
        myReactor.setName(mapReactor.Name);
        myReactor.resetReactorActions(0);
        return myReactor;
    }

    private static string getMapName(int mapid)
    {
        string mapName = StringUtil.getLeftPaddedStr(mapid.ToString(), '0', 9);
        StringBuilder builder = new StringBuilder("Map/Map");
        int area = mapid / 100000000;
        builder.Append(area);
        builder.Append("/");
        builder.Append(mapName);
        builder.Append(".img");
        mapName = builder.ToString();
        return mapName;
    }

    private static string getMapStringName(int mapid)
    {
        StringBuilder builder = new StringBuilder();
        if (mapid < 100000000)
        {
            builder.Append("maple");
        }
        else if (mapid >= 100000000 && mapid < MapId.ORBIS)
        {
            builder.Append("victoria");
        }
        else if (mapid >= MapId.ORBIS && mapid < MapId.ELLIN_FOREST)
        {
            builder.Append("ossyria");
        }
        else if (mapid >= MapId.ELLIN_FOREST && mapid < 400000000)
        {
            builder.Append("elin");
        }
        else if (mapid >= MapId.SINGAPORE && mapid < 560000000)
        {
            builder.Append("singapore");
        }
        else if (mapid >= MapId.NEW_LEAF_CITY && mapid < 620000000)
        {
            builder.Append("MasteriaGL");
        }
        else if (mapid >= 677000000 && mapid < 677100000)
        {
            builder.Append("Episode1GL");
        }
        else if (mapid >= 670000000 && mapid < 682000000)
        {
            if ((mapid >= 674030000 && mapid < 674040000) || (mapid >= 680100000 && mapid < 680200000))
            {
                builder.Append("etc");
            }
            else
            {
                builder.Append("weddingGL");
            }
        }
        else if (mapid >= 682000000 && mapid < 683000000)
        {
            builder.Append("HalloweenGL");
        }
        else if (mapid >= 683000000 && mapid < 684000000)
        {
            builder.Append("event");
        }
        else if (mapid >= MapId.MUSHROOM_SHRINE && mapid < 900000000)
        {
            if ((mapid >= 889100000 && mapid < 889200000))
            {
                builder.Append("etc");
            }
            else
            {
                builder.Append("jp");
            }
        }
        else
        {
            builder.Append("etc");
        }
        builder.Append("/").Append(mapid);
        return builder.ToString();
    }

    public static string loadPlaceName(int mapid)
    {
        try
        {
            return DataTool.getString("mapName", nameData.getChildByPath(getMapStringName(mapid))) ?? "";
        }
        catch (Exception e)
        {
            LogFactory.ResLogger.Error(e.ToString());
            return "";
        }
    }

    public static string loadStreetName(int mapid)
    {
        try
        {
            return DataTool.getString("streetName", nameData.getChildByPath(getMapStringName(mapid))) ?? "";
        }
        catch (Exception e)
        {
            LogFactory.ResLogger.Error(e.ToString());
            return "";
        }
    }

}
