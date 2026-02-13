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
    private Point? position = new Point();
    private int objectId;

    public abstract MapObjectType getType();

    public virtual Point getPosition()
    {
        return position ?? Point.Empty;
    }

    public virtual void setPosition(Point position)
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

    public virtual void nullifyPosition()
    {
        this.position = null;
    }

    public virtual void sendSpawnData(IChannelClient client) { }
    public virtual void sendDestroyData(IChannelClient client) { }

    /// <summary>
    /// 相同MapId，不同频道的Map也不一样
    /// </summary>
    public IMap MapModel { get; private set; } = null!;
    public virtual IMap getMap()
    {
        return MapModel;
    }

    public virtual void setMap(IMap map)
    {
        this.MapModel = map;
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

    public virtual void MapRemove()
    {
        getMap().removeMapObject(this);
        getMap().BroadcastAll(chr => sendDestroyData(chr.Client));
    }
    /// <summary>
    /// 是否能被玩家看到
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    protected virtual bool IsPlayerVisiable(Player chr)
    {
        return true;
    }

    public virtual void Enter(IMap map, Action<Player> chrAction)
    {
        map.addMapObject(this);
        setMap(map);

        foreach (Player chr in map.getAllPlayers())
        {
            if (IsPlayerVisiable(chr) && (getType().IsNonRangedType() || chr.getPosition().distanceSq(getPosition()) <= MapleMap.getRangedDistance()))
            {
                chr.addVisibleMapObject(this);
                chrAction(chr);
            }
        }
    }

    public virtual void Leave(Action<Player> chrAction)
    {
        MapModel.removeMapObject(this);

        foreach (Player chr in MapModel.getAllPlayers())
        {
            if (IsPlayerVisiable(chr) && (getType().IsNonRangedType() || chr.getPosition().distanceSq(getPosition()) <= MapleMap.getRangedDistance()))
            {
                chr.removeVisibleMapObject(this);
                chrAction(chr);
            }
        }
    }
}
