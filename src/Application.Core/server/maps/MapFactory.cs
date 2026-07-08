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


using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Maps.Specials;
using Application.Core.scripting.Events.Instances;
using Application.Templates.Map;
using Application.Templates.Reader;
using Microsoft.Extensions.DependencyInjection;
using server.life;
using server.partyquest;
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

    private IProvider<MapTemplate> mapSource;

    public MapFactory()
    {
        mapSource = ProviderSource.Instance.GetProvider<IProvider<MapTemplate>>(ProviderType.Map);
    }

    private AbstractLifeObject? loadLife(IMap map, int id, string type, int cy, int f, int fh, int rx0, int rx1, int x, int y, bool hide)
    {
        AbstractLifeObject? myLife = null;
        if (type.Equals(LifeType.NPC, StringComparison.OrdinalIgnoreCase))
        {
            var npcData = LifeFactory.Instance.getNPC(id);
            if (npcData != null)
            {
                myLife = map.CreateNPC(npcData, new Point(x, y));
            }
        }
        else if (type.Equals(LifeType.Monster, StringComparison.OrdinalIgnoreCase))
        {
            var mobData = LifeFactory.Instance.getMonster(id);
            if (mobData != null)
            {
                myLife = map.CreateMonster(mobData, new Point(x, y));
            }

        }
        else
        {
            Log.Logger.Warning("Unknown Life type: {LifeType}", type);
            return null;
        }

        if (myLife != null)
        {
            myLife.setCy(cy);
            myLife.setF(f);
            myLife.setFh(fh);
            myLife.setRx0(rx0);
            myLife.setRx1(rx1);
            myLife.setPosition(new Point(x, y));
            myLife.setHide(hide);
        }

        return myLife;
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
        var dataList = map.ChannelServer.NodeService.DataService.LoadPLife(map.Id);

        foreach (var rs in dataList)
        {
            loadLifeRaw(map, rs.LifeId, rs.Type, rs.Cy, rs.F, rs.Fh, rs.Rx0, rs.Rx1, rs.X, rs.Y, rs.Hide == 1, rs.Mobtime, rs.Team);
        }
    }

    private void loadLifeRaw(IMap map, int id, string type, int cy, int f, int fh, int rx0, int rx1, int x, int y, bool hide, int mobTime, int team)
    {
        if (type == LifeType.Monster)
        {
            map.addMonsterSpawn(id, new Point(x, y), cy, f, fh, rx0, rx1, mobTime, hide, team);
        }
        else
        {
            var mapObj = loadLife(map, id, type, cy, f, fh, rx0, rx1, x, y, hide);
            if (mapObj != null)
            {
                // 初始化时无玩家
                map.AddMapObject(mapObj, null);
            }
        }
    }

    public MapTemplate GetMapTemplate(int mapId) => mapSource.GetItem(mapId) ?? throw new BusinessResException($"Map {mapId} not existed");

    public async Task<IMap> loadMapFromWz(int mapid, WorldChannel worldChannel, AbstractEventInstanceManager? evt)
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
            map.ChannelServer.NodeService.PlayerNPCService.LoadPlayerNpc(map);
        }

        loadLifeFromWz(map, mapData);
        loadLifeFromDb(map);


        foreach (var item in mapData.Reactors)
        {
            Reactor newReactor = loadReactor(map, item);
            await map.spawnReactor(newReactor);
        }

        if (mapData.ReactorShuffle)
        {
            map.shuffleReactors();
        }


        map.generateMapDropRangeCache();

        return map;
    }

    private Reactor loadReactor(IMap map, MapReactorTemplate mapReactor)
    {
        Reactor myReactor = new Reactor(map, new Point(mapReactor.X, mapReactor.Y), ReactorFactory.getReactor(mapReactor.Id), mapReactor.Id);
        myReactor.setFacingDirection((sbyte)mapReactor.F);
        myReactor.setDelay(mapReactor.ReactorTime * 1000);
        myReactor.setName(mapReactor.Name ?? "");
        _ = myReactor.resetReactorActions(0);
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
