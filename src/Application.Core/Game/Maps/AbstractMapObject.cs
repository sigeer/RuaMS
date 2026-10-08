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

    public virtual Task sendSpawnData(IChannelClient client) { return Task.CompletedTask; }
    public virtual Task sendDestroyData(IChannelClient client) { return Task.CompletedTask; }


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


    public virtual async Task OnMounted(IMap map)
    {
        MapModel = map;

        await map.ChannelServer.NodeService.PluginManager.OnMapObjectEnterField(map, this);
    }

    public virtual async Task OnUnmounted()
    {
        await MapModel.ChannelServer.NodeService.PluginManager.OnMapObjectLeaveField(MapModel, this);
    }

    public virtual VisionType LifeScopeLevel => VisionType.InVision;
    public virtual VisionType MessageScopeLevel => VisionType.InVision;


    public virtual VisionType GetVisionTypeForPlayer(Player chr)
    {
        if (MapModel == chr.MapModel)
        {
            if (!MapModel.UseRangedView || MapGlobalData.IsObjectInRange(this, chr.getPosition(), MapModel.ChannelServer.NodeService.NodeConfig.SystemConfig.GetRangedDistance()))
            {
                return VisionType.InVision;
            }
        }

        return VisionType.OutofVision;
    }

    public async Task BroadcastMap(Packet packet, int exceptCId = -1)
    {
        foreach (var mapChr in MapModel.getAllPlayers())
        {
            if (mapChr.Id == exceptCId)
            {
                continue;
            }


            if (MapModel.GetVisionTypeForPlayerCached(mapChr, this) >= MessageScopeLevel)
            {
                await mapChr.SendPacket(packet);
            }
        }
    }
}
