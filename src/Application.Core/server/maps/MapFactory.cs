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


using Acornima.Ast;
using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Maps.Specials;
using Application.Resources;
using Application.Shared.MapObjects;
using Application.Shared.Objects;
using Application.Shared.WzEntity;
using Application.Templates.Map;
using Application.Templates.Providers;
using Application.Templates.String;
using Application.Templates.XmlWzReader.Provider;
using Microsoft.Extensions.DependencyInjection;
using scripting.Event;
using server.life;
using server.partyquest;
using System.Numerics;
using System.Text;
using tools;


namespace server.maps;

public class MapFactory : IStaticService
{
    private static MapFactory? _instance;

    public static MapFactory Instance => _instance ?? throw new BusinessFatalException("MapFactory 未注册");

    public void Register(IServiceProvider sp)
    {
        if (_instance != null)
            return;

        _instance = sp.GetService<MapFactory>() ?? throw new BusinessFatalException("MapFactory 未注册");
    }

    private MapProvider mapSource;

    public MapFactory()
    {
        mapSource = ProviderFactory.GetProvider<MapProvider>();
    }

    private void loadLifeFromWz(IMap map, MapTemplate mapData)
    {
        foreach (var life in mapData.Life)
        {
            int team = life.Team;
            if (map.isCPQMap2() && life.Type == LifeType.Monster)
            {
                if ((life.Index % 2) == 0)
                {
                    team = 0;
                }
                else
                {
                    team = 1;
                }
            }

            loadLifeRaw(map, life.Id, life.Type, life.CY, life.F, life.Foothold, life.RX0, life.RX1, life.X, life.Y, life.Hide, life.MobTime, team);
        }
    }

    private void loadLifeFromDb(IMap map)
    {
        var dataList = map.ChannelServer.Container.DataService.LoadPLife(map.Id);

        foreach (var rs in dataList)
        {
            loadLifeRaw(map, rs.LifeId, rs.Type, rs.Cy, rs.F, rs.Fh, rs.Rx0, rs.Rx1, rs.X, rs.Y, rs.Hide == 1, rs.Mobtime, rs.Team);
        }
    }

    private void loadLifeRaw(IMap map, int id, string type, int cy, int f, int fh, int rx0, int rx1, int x, int y, bool hide, int mobTime, int team)
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

    public IMap loadMapFromWz(int mapid, WorldChannel worldChannel, EventInstanceManager? evt)
    {
        IMap? map = null;

        var mapData = mapSource.GetItem(mapid); 
        if (mapData == null)
            throw new BusinessResException($"Map {mapid} not existed");


        if (MapConstants.IsCPQMap(mapid) && mapData.MonsterCarnival != null)
            map = new MonsterCarnivalMap(mapData, worldChannel, evt);
        else if (mapData.Coconut != null)
            map = new CoconutMap(mapData, worldChannel, evt);
        else if (mapData.Snowball != null)
            map = new SnowBallMap(mapData, worldChannel, evt);
        else
            map = new MapleMap(mapData, worldChannel, evt);

        loadLifeFromWz(map, mapData);
        loadLifeFromDb(map);

        return map;
    }

    private AbstractLifeObject loadLife(int id, string type, int cy, int f, int fh, int rx0, int rx1, int x, int y, bool hide)
    {
        AbstractLifeObject myLife = LifeFactory.Instance.getLife(id, type) ?? throw new BusinessResException($"LifeFactory.getLife({id}, {type})");
        myLife.setCy(cy);
        myLife.setF(f);
        myLife.setFh(fh);
        myLife.setRx0(rx0);
        myLife.setRx1(rx1);
        myLife.setPosition(new Point(x, y));
        myLife.setHide(hide);
        return myLife;
    }


    public string loadPlaceName(int mapid) => ClientCulture.SystemCulture.GetMapName(mapid);

    public string loadStreetName(int mapid) => ClientCulture.SystemCulture.GetMapStreetName(mapid);

}
