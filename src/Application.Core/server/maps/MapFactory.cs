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


using Application.Core.constants.game;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Maps.Specials;
using constants.id;
using provider;
using provider.wz;
using scripting.Event;
using server.life;
using server.partyquest;
using System.Text;
using tools;


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

    private static void loadLifeFromWz(IMap map, Data mapData)
    {
        foreach (var life in mapData.getChildByPath("life"))
        {
            var id = DataTool.getString(life.getChildByPath("id")) ?? "0";
            var type = DataTool.getString(life.getChildByPath("type"));
            int team = DataTool.getInt("team", life, -1);
            if (map.isCPQMap2() && type == LifeType.Monster)
            {
                if ((int.Parse(life.getName()) % 2) == 0)
                {
                    team = 0;
                }
                else
                {
                    team = 1;
                }
            }
            int cy = DataTool.getInt(life.getChildByPath("cy"));
            var dF = life.getChildByPath("f");
            int f = (dF != null) ? DataTool.getInt(dF) : 0;
            int fh = DataTool.getInt(life.getChildByPath("fh"));
            int rx0 = DataTool.getInt(life.getChildByPath("rx0"));
            int rx1 = DataTool.getInt(life.getChildByPath("rx1"));
            int x = DataTool.getInt(life.getChildByPath("x"));
            int y = DataTool.getInt(life.getChildByPath("y"));
            int hide = DataTool.getInt("hide", life, 0);
            int mobTime = DataTool.getInt("mobTime", life, 0);

            loadLifeRaw(map, int.Parse(id), type, cy, f, fh, rx0, rx1, x, y, hide, mobTime, team);
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

                loadLifeRaw(map, id, type, cy, f, fh, rx0, rx1, x, y, hide, mobTime, team);
            }

        }
        catch (Exception sqle)
        {
            Log.Logger.Error(sqle.ToString());
        }
    }

    private static void loadLifeRaw(IMap map, int id, string type, int cy, int f, int fh, int rx0, int rx1, int x, int y, int hide, int mobTime, int team)
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
        float monsterRate = 0;
        var mobRate = infoData?.getChildByPath("mobRate");
        if (mobRate != null)
        {
            monsterRate = (float?)mobRate.getData() ?? 0;
        }
        map = new MapleMap(mapid, world, channel, DataTool.getInt("returnMap", infoData), monsterRate);

        var cpqInfo = mapData.getChildByPath("monsterCarnival");
        if (map.isCPQMap() && cpqInfo != null)
            map = new MonsterCarnivalMap(map);

        map.setEventInstance(evt);

        var mapStr = mapid.ToString();
        string onFirstEnter = DataTool.getString(infoData?.getChildByPath("onFirstUserEnter")) ?? mapStr;
        map.setOnFirstUserEnter(onFirstEnter.Equals("") ? mapStr : onFirstEnter);

        string onEnter = DataTool.getString(infoData?.getChildByPath("onUserEnter")) ?? mapStr;
        map.setOnUserEnter(onEnter.Equals("") ? mapStr : onEnter);

        map.setFieldLimit(DataTool.getInt(infoData?.getChildByPath("fieldLimit"), 0));
        map.setMobInterval((short)DataTool.getInt(infoData?.getChildByPath("createMobInterval"), 5000));
        PortalFactory portalFactory = new PortalFactory();
        foreach (var portal in mapData.getChildByPath("portal"))
        {
            map.addPortal(portalFactory.makePortal(DataTool.getInt(portal.getChildByPath("pt")), portal));
        }
        var timeMob = infoData?.getChildByPath("timeMob");
        if (timeMob != null)
        {
            map.setTimeMob(DataTool.getInt(timeMob.getChildByPath("id")), DataTool.getString(timeMob.getChildByPath("message")));
        }

        int[] bounds = new int[4];
        bounds[0] = DataTool.getInt(infoData?.getChildByPath("VRTop"));
        bounds[1] = DataTool.getInt(infoData?.getChildByPath("VRBottom"));

        if (bounds[0] == bounds[1])
        {    // old-style baked map
            var minimapData = mapData.getChildByPath("miniMap");
            if (minimapData != null)
            {
                bounds[0] = DataTool.getInt(minimapData.getChildByPath("centerX")) * -1;
                bounds[1] = DataTool.getInt(minimapData.getChildByPath("centerY")) * -1;
                bounds[2] = DataTool.getInt(minimapData.getChildByPath("height"));
                bounds[3] = DataTool.getInt(minimapData.getChildByPath("width"));

                map.setMapPointBoundings(bounds[0], bounds[1], bounds[2], bounds[3]);
            }
            else
            {
                int dist = (1 << 18);
                map.setMapPointBoundings(-dist / 2, -dist / 2, dist, dist);
            }
        }
        else
        {
            bounds[2] = DataTool.getInt(infoData?.getChildByPath("VRLeft"));
            bounds[3] = DataTool.getInt(infoData?.getChildByPath("VRRight"));

            map.setMapLineBoundings(bounds[0], bounds[1], bounds[2], bounds[3]);
        }

        List<Foothold> allFootholds = new();
        Point lBound = new Point();
        Point uBound = new Point();
        foreach (var footRoot in mapData.getChildByPath("foothold"))
        {
            foreach (Data footCat in footRoot)
            {
                foreach (Data footHold in footCat)
                {
                    int x1 = DataTool.getInt(footHold.getChildByPath("x1"));
                    int y1 = DataTool.getInt(footHold.getChildByPath("y1"));
                    int x2 = DataTool.getInt(footHold.getChildByPath("x2"));
                    int y2 = DataTool.getInt(footHold.getChildByPath("y2"));
                    Foothold fh = new Foothold(new Point(x1, y1), new Point(x2, y2), int.Parse(footHold.getName()));
                    fh.setPrev(DataTool.getInt(footHold.getChildByPath("prev")));
                    fh.setNext(DataTool.getInt(footHold.getChildByPath("next")));
                    if (fh.getX1() < lBound.X)
                    {
                        lBound.X = fh.getX1();
                    }
                    if (fh.getX2() > uBound.X)
                    {
                        uBound.X = fh.getX2();
                    }
                    if (fh.getY1() < lBound.Y)
                    {
                        lBound.Y = fh.getY1();
                    }
                    if (fh.getY2() > uBound.Y)
                    {
                        uBound.Y = fh.getY2();
                    }
                    allFootholds.Add(fh);
                }
            }
        }
        FootholdTree fTree = new FootholdTree(lBound, uBound);
        foreach (Foothold fh in allFootholds)
        {
            fTree.insert(fh);
        }
        map.setFootholds(fTree);
        if (mapData.getChildByPath("area") != null)
        {
            foreach (var area in mapData.getChildByPath("area")!)
            {
                int x1 = DataTool.getInt(area.getChildByPath("x1"));
                int y1 = DataTool.getInt(area.getChildByPath("y1"));
                int x2 = DataTool.getInt(area.getChildByPath("x2"));
                int y2 = DataTool.getInt(area.getChildByPath("y2"));
                map.addMapleArea(new Rectangle(x1, y1, (x2 - x1), (y2 - y1)));
            }
        }
        if (mapData.getChildByPath("seat") != null)
        {
            int seats = mapData.getChildByPath("seat")!.getChildren().Count;
            map.setSeats(seats);
        }
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

        loadLifeFromWz(map, mapData);
        loadLifeFromDb(map);

        if (map is MonsterCarnivalMap cpqMap)
        {
            cpqMap.DeathCP = (DataTool.getIntConvert("deathCP", cpqInfo, 0));
            cpqMap.MaxMobs = (DataTool.getIntConvert("mobGenMax", cpqInfo, 20));    // thanks Atoot for noticing CPQ1 bf. 3 and 4 not accepting spawns due to undefined limits, Lame for noticing a need to cap mob spawns even on such undefined limits
            cpqMap.TimeDefault = (DataTool.getIntConvert("timeDefault", cpqInfo, 0));
            cpqMap.TimeExpand = (DataTool.getIntConvert("timeExpand", cpqInfo, 0));
            cpqMap.MaxReactors = (DataTool.getIntConvert("guardianGenMax", cpqInfo, 16));
            var guardianGenData = cpqInfo!.getChildByPath("guardianGenPos");
            foreach (Data node in guardianGenData.getChildren())
            {
                GuardianSpawnPoint pt = new GuardianSpawnPoint(new Point(DataTool.getIntConvert("x", node), DataTool.getIntConvert("y", node)));
                pt.setTeam(DataTool.getIntConvert("team", node, -1));
                pt.setTaken(false);
                cpqMap.AddGuardianSpawnPoint(pt);
            }
            if (cpqInfo.getChildByPath("skill") != null)
            {
                foreach (var area in cpqInfo.getChildByPath("skill")!)
                {
                    cpqMap.AddSkillId(DataTool.getInt(area));
                }
            }

            if (cpqInfo.getChildByPath("mob") != null)
            {
                foreach (var area in cpqInfo.getChildByPath("mob")!)
                {
                    cpqMap.AddMobSpawn(DataTool.getInt(area.getChildByPath("id")), DataTool.getInt(area.getChildByPath("spendCP")));
                }
            }

        }

        if (mapData.getChildByPath("reactor") != null)
        {
            foreach (var reactor in mapData.getChildByPath("reactor")!)
            {
                var id = DataTool.getString(reactor.getChildByPath("id"));
                if (id != null)
                {
                    Reactor newReactor = loadReactor(reactor, id, (byte)DataTool.getInt(reactor.getChildByPath("f"), 0));
                    map.spawnReactor(newReactor);
                }
            }
        }

        map.setMapName(loadPlaceName(mapid));
        map.setStreetName(loadStreetName(mapid));

        map.setClock(mapData.getChildByPath("clock") != null);
        map.setEverlast(DataTool.getIntConvert("everlast", infoData, 0) != 0); // thanks davidlafriniere for noticing value 0 accounting as true
        map.setTown(DataTool.getIntConvert("town", infoData, 0) != 0);
        map.setHPDec(DataTool.getIntConvert("decHP", infoData, 0));
        map.setHPDecProtect(DataTool.getIntConvert("protectItem", infoData, 0));
        map.setForcedReturnMap(DataTool.getInt(infoData?.getChildByPath("forcedReturn"), MapId.NONE));
        map.setBoat(mapData.getChildByPath("shipObj") != null);
        map.setTimeLimit(DataTool.getIntConvert("timeLimit", infoData, -1));
        map.setFieldType(DataTool.getIntConvert("fieldType", infoData, 0));
        map.setMobCapacity(DataTool.getIntConvert("fixedMobCapacity", infoData, 500));//Is there a map that contains more than 500 mobs?

        var recData = infoData?.getChildByPath("recovery");
        if (recData != null)
        {
            map.setRecovery(DataTool.getFloat(recData));
        }

        Dictionary<int, int> backTypes = new();
        try
        {
            foreach (var layer in mapData.getChildByPath("back"))
            { 
                // yolo
                int layerNum = int.Parse(layer.getName());
                int btype = DataTool.getInt(layer.getChildByPath("type"), 0);

                backTypes.AddOrUpdate(layerNum, btype);
            }
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
            // swallow cause I'm cool
        }

        map.setBackgroundTypes(backTypes);
        map.generateMapDropRangeCache();

        return map;
    }

    private static AbstractLifeObject loadLife(int id, string type, int cy, int f, int fh, int rx0, int rx1, int x, int y, int hide)
    {
        AbstractLifeObject myLife = LifeFactory.getLife(id, type) ?? throw new BusinessResException($"LifeFactory.getLife({id}, {type})");
        myLife.setCy(cy);
        myLife.setF(f);
        myLife.setFh(fh);
        myLife.setRx0(rx0);
        myLife.setRx1(rx1);
        myLife.setPosition(new Point(x, y));
        if (hide == 1)
        {
            myLife.setHide(true);
        }
        return myLife;
    }

    private static Reactor loadReactor(Data reactor, string id, byte FacingDirection)
    {
        Reactor myReactor = new Reactor(ReactorFactory.getReactor(int.Parse(id)), int.Parse(id));
        int x = DataTool.getInt(reactor.getChildByPath("x"));
        int y = DataTool.getInt(reactor.getChildByPath("y"));
        myReactor.setFacingDirection(FacingDirection);
        myReactor.setPosition(new Point(x, y));
        myReactor.setDelay((DataTool.getInt(reactor.getChildByPath("reactorTime"))) * 1000);
        myReactor.setName(DataTool.getString(reactor.getChildByPath("name")) ?? "");
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
