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



using Application.Core.Game.Maps;
using Application.Core.Game.TheWorld;
using scripting.Event;
using System.Collections.Concurrent;

namespace server.maps;

public class MapManager : IDisposable
{
    private EventInstanceManager? evt;
    readonly IWorldChannel _channelServer;

    private ConcurrentDictionary<int, IMap> maps = new();

    public MapManager(EventInstanceManager? eim, IWorldChannel worldChannel)
    {
        _channelServer = worldChannel;
        this.evt = eim;
    }

    public IMap resetMap(int mapid)
    {
        maps.Remove(mapid);

        return getMap(mapid);
    }

    object loadWZLock = new object();
    private IMap loadMapFromWz(int mapid, bool cache)
    {
        lock (loadWZLock)
        {
            IMap? map;

            if (cache)
            {
                map = maps.GetValueOrDefault(mapid);

                if (map != null)
                {
                    return map;
                }
            }

            map = MapFactory.loadMapFromWz(mapid, _channelServer, evt);

            if (cache)
            {
                maps.AddOrUpdate(mapid, map);
            }

            return map;
        }

    }

    public IMap getMap(int mapid)
    {
        return loadMapFromWz(mapid, true);
    }

    public IMap getDisposableMap(int mapid)
    {
        return loadMapFromWz(mapid, false);
    }

    public bool isMapLoaded(int mapId)
    {
        return maps.ContainsKey(mapId);
    }

    public Dictionary<int, IMap> getMaps()
    {
        return new(maps);
    }

    public void updateMaps()
    {
        foreach (IMap map in getMaps().Values)
        {
            map.respawn();
            map.mobMpRecovery();
        }
    }

    public void Dispose()
    {
        foreach (IMap map in getMaps().Values)
        {
            map.Dispose();
        }

        this.evt = null;
    }

}
