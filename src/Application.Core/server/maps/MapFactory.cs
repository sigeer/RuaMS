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
using Application.Core.Scripting.Events;
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
        mapSource = ProviderSource.Instance.GetProvider<MapProvider>();
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

    public IMap loadMapFromWz(int mapid, WorldChannel worldChannel, AbstractEventInstanceManager? evt)
    {
        IMap? map = null;

        var mapData = mapSource.GetItem(mapid); 
        if (mapData == null)
            throw new BusinessResException($"Map {mapid} not existed");


        if (MapConstants.IsCPQMap(mapid) && mapData.MonsterCarnival != null)
        {
            var cpqMap = new MonsterCarnivalMap(mapData, worldChannel, evt);
            var mcData = mapData.MonsterCarnival;
            cpqMap.DeathCP = mcData.DeathCP;
            cpqMap.MaxMobs = mcData.MaxMobs;    // thanks Atoot for noticing CPQ1 bf. 3 and 4 not accepting spawns due to undefined limits, Lame for noticing a need to cap mob spawns even on such undefined limits

            cpqMap.MaxReactors = mcData.MaxReactors;

            cpqMap.RewardMapWin = mcData.RewardMapWin;
            cpqMap.RewardMapLose = mcData.RewardMapLose;
            cpqMap.ReactorRed = mcData.ReactorRed;
            cpqMap.ReactorBlue = mcData.ReactorBlue;

            cpqMap.TimeDefault = mcData.TimeDefault;
            cpqMap.TimeExpand = mcData.TimeExpand;
            cpqMap.TimeFinish = mcData.TimeFinish;

            cpqMap.SoundWin = mcData.SoundWin ?? cpqMap.GetDefaultSoundWin();
            cpqMap.SoundLose = mcData.SoundLose ?? cpqMap.GetDefaultSoundLose();
            cpqMap.EffectWin = mcData.EffectWin ?? cpqMap.GetDefaultEffectWin();
            cpqMap.EffectLose = mcData.EffectLose ?? cpqMap.GetDefaultEffectLose();

            foreach (var item in mcData.Guardians)
            {
                GuardianSpawnPoint pt = new GuardianSpawnPoint(new Point(item.X, item.Y));
                pt.setTeam(item.Team);
                pt.setTaken(false);
                cpqMap.AddGuardianSpawnPoint(pt);
            }

            foreach (var item in mcData.Skills)
            {
                cpqMap.AddSkillId(item);
            }

            foreach (var item in mcData.Mobs)
            {
                cpqMap.AddMobSpawn(item.Id, item.SpendCP);
            }

            map = cpqMap;
        }

        else if (mapData.Coconut != null)
        {
            var coconutMap = new CoconutMap(mapData, worldChannel, evt);
            coconutMap.CountFalling = mapData.Coconut.CountFalling;
            coconutMap.CountBombing = mapData.Coconut.CountBombing;
            coconutMap.CountStopped = mapData.Coconut.CountStopped;
            coconutMap.CountHit = mapData.Coconut.CountHit;

            coconutMap.TimeDefault = mapData.Coconut.TimeDefault;
            coconutMap.TimeExpand = mapData.Coconut.TimeExpand;
            coconutMap.TimeFinish = mapData.Coconut.TimeFinish;

            coconutMap.SoundWin = mapData.Coconut.SoundWin ?? coconutMap.GetDefaultSoundWin();
            coconutMap.SoundLose = mapData.Coconut.SoundLose ?? coconutMap.GetDefaultSoundLose();
            coconutMap.EffectWin = mapData.Coconut.EffectWin ?? coconutMap.GetDefaultEffectWin();
            coconutMap.EffectLose = mapData.Coconut.EffectLose ?? coconutMap.GetDefaultEffectLose();
            map = coconutMap;
        }

        else if (mapData.Snowball != null)
        {
            var snowBallMap = new SnowBallMap(mapData, worldChannel, evt);
            snowBallMap.DamageSnowBall = mapData.Snowball.DamageSnowBall;
            snowBallMap.DamageSnowMan0 = mapData.Snowball.DamageSnowMan0;
            snowBallMap.DamageSnowMan1 = mapData.Snowball.DamageSnowMan1;
            snowBallMap.RecoveryAmount = mapData.Snowball.RecoveryAmount;
            snowBallMap.SnowManHP = mapData.Snowball.SnowManHP;
            snowBallMap.SnowManWait = mapData.Snowball.SnowManWait;
            map = snowBallMap;
        }

        else
            map = new MapleMap(mapData, worldChannel, evt);

        if (evt == null)
        {
            map.ChannelServer.Container.PlayerNPCService.LoadPlayerNpc(map);
        }

        loadLifeFromWz(map, mapData);
        loadLifeFromDb(map);


        foreach (var item in mapData.Reactors)
        {
            Reactor newReactor = loadReactor(item);
            map.spawnReactor(newReactor);
        }

        map.setMapName(loadPlaceName(mapid));
        map.setStreetName(loadStreetName(mapid));

        map.generateMapDropRangeCache();

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

    private Reactor loadReactor(Data mapReactor, string id, sbyte FacingDirection)
    {
        var reactorId = int.Parse(id);
        Reactor myReactor = new Reactor(ReactorFactory.getReactor(reactorId), reactorId);
        int x = DataTool.getInt(mapReactor.getChildByPath("x"));
        int y = DataTool.getInt(mapReactor.getChildByPath("y"));
        myReactor.setFacingDirection(FacingDirection);
        myReactor.setPosition(new Point(x, y));
        myReactor.setDelay((DataTool.getInt(mapReactor.getChildByPath("reactorTime"))) * 1000);
        myReactor.setName(DataTool.getString(mapReactor.getChildByPath("name")) ?? "");
        myReactor.resetReactorActions(0);
        return myReactor;
    }

    private Reactor loadReactor(MapReactorTemplate mapReactor)
    {
        Reactor myReactor = new Reactor(ReactorFactory.getReactor(mapReactor.Id), mapReactor.Id);
        myReactor.setFacingDirection((sbyte)mapReactor.F);
        myReactor.setPosition(new Point(mapReactor.X, mapReactor.Y));
        myReactor.setDelay(mapReactor.ReactorTime * 1000);
        myReactor.setName(mapReactor.Name ?? "");
        myReactor.resetReactorActions(0);
        return myReactor;
    }

    private string GetMapImg(int mapid)
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

    public string loadPlaceName(int mapid) => ClientCulture.SystemCulture.GetMapName(mapid);

    public string loadStreetName(int mapid) => ClientCulture.SystemCulture.GetMapStreetName(mapid);

}
