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
}
