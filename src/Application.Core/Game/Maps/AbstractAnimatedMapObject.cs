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



public abstract class AbstractAnimatedMapObject : AbstractMapObject, IAnimatedMapObject
{

    private int stance;

    public int getStance()
    {
        return stance;
    }

    public void setStance(int stance)
    {
        this.stance = stance;
    }

    public virtual bool isFacingLeft()
    {
        return Math.Abs(stance) % 2 == 1;
    }

    public byte[] GetIdleMovementBytes()
    {
        var pos = getPosition();
        var stance = getStance();

        return new byte[]
        {
            1,
            0,
            (byte)(pos.X & 0xFF), (byte)(pos.X >> 8 & 0xFF),
            (byte)(pos.Y & 0xFF), (byte)(pos.Y >> 8 & 0xFF),
            0, 0,
            0, 0,
            0, 0,
            (byte)(stance & 0xFF),
            0, 0
        };

    }
}
