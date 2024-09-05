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
using scripting.Event;

namespace server.maps;

public class MapManager
{
    private int channel;
    private int world;
    private EventInstanceManager? evt;

    private Dictionary<int, IMap> maps = new();

    ReaderWriterLockSlim mapsLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    public MapManager(EventInstanceManager eim, int world, int channel)
    {
        this.world = world;
        this.channel = channel;
        this.evt = eim;
    }

    public IMap resetMap(int mapid)
    {
        mapsLock.EnterWriteLock();
        try
        {
            maps.Remove(mapid);
        }
        finally
        {
            mapsLock.ExitWriteLock();
        }

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
                mapsLock.EnterReadLock();
                try
                {
                    map = maps.GetValueOrDefault(mapid);
                }
                finally
                {
                    mapsLock.ExitReadLock();
                }

                if (map != null)
                {
                    return map;
                }
            }

            map = MapFactory.loadMapFromWz(mapid, world, channel, evt);

            if (cache)
            {
                mapsLock.EnterWriteLock();
                try
                {
                    maps.AddOrUpdate(mapid, map);
                }
                finally
                {
                    mapsLock.ExitWriteLock();
                }
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
        mapsLock.EnterReadLock();
        try
        {
            return maps.ContainsKey(mapId);
        }
        finally
        {
            mapsLock.ExitReadLock();
        }
    }

    public Dictionary<int, IMap> getMaps()
    {
        mapsLock.EnterReadLock();
        try
        {
            return new(maps);
        }
        finally
        {
            mapsLock.ExitReadLock();
        }
    }

    public void updateMaps()
    {
        foreach (IMap map in getMaps().Values)
        {
            map.respawn();
            map.mobMpRecovery();
        }
    }

    public void dispose()
    {
        foreach (IMap map in getMaps().Values)
        {
            map.dispose();
        }

        this.evt = null;
    }

}
