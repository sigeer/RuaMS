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
namespace Application.Core.Game.Maps;

public abstract class AbstractMapObject : IMapObject
{
    /// <summary>
    /// 相同MapId，不同频道的Map也不一样
    /// </summary>
    public IMap MapModel { get; private set; }
    private Point position;
    private int objectId;

    protected AbstractMapObject(IMap mapModel, Point position)
    {
        MapModel = mapModel;
        this.position = position;
    }

    public abstract MapObjectType getType();

    public virtual Point getPosition()
    {
        return position;
    }

    public void setPosition(Point position)
    {
        this.position = position;
    }

    public virtual int getObjectId()
    {
        return objectId;
    }

    public virtual void setObjectId(int id)
    {
        this.objectId = id;
    }

    public virtual void sendSpawnData(IChannelClient client) { }
    public virtual void sendDestroyData(IChannelClient client) { }


    public virtual IMap getMap()
    {
        return MapModel;
    }

    public virtual string GetName()
    {
        return getType().ToString();
    }

    public virtual string GetReadableName(IChannelClient c)
    {
        return GetName();
    }
    public virtual int GetSourceId()
    {
        return getObjectId();
    }


    public virtual void OnMounted(IMap map)
    {
        MapModel = map;

        map.ChannelServer.NodeService.PluginManager.OnMapObjectEnterField(map, this);
    }

    public virtual void OnUnmounted()
    {
        MapModel.ChannelServer.NodeService.PluginManager.OnMapObjectLeaveField(MapModel, this);
    }

    /// <summary>
    /// 不考虑距离的情况下，对玩家是否可视
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    protected virtual bool IsVisibleForPlayerWithoutRange(Player chr) => MapModel == chr.MapModel;
    public virtual bool IsVisibleForPlayer(Player chr)
    {
        return IsVisibleForPlayerWithoutRange(chr) &&
            (!MapModel.UseRangedView || MapGlobalData.IsObjectInRange(this, chr.getPosition(), MapModel.ChannelServer.NodeService.ServerConfig.SystemConfig.GetRangedDistance()));
    }


    public void BroadcastMap(Packet packet, int exceptCId = -1)
    {
        foreach (var mapChr in MapModel.getAllPlayers())
        {
            if (mapChr.Id == exceptCId)
            {
                continue;
            }


            if (MapModel.IsMapObjectVisibleForPlayerCached(mapChr, this))
            {
                mapChr.sendPacket(packet);
            }
        }
    }
}
