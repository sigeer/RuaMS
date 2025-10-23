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
using System.Drawing;

namespace Application.Shared.MapObjects;



/**
 * @author Matze
 */
public class Foothold : IComparable<Foothold>, IEquatable<Foothold>
{
    private Point p1;
    private Point p2;
    private int id;
    private int next, prev;

    public Foothold(Point p1, Point p2, int id)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.id = id;
    }

    public bool isWall()
    {
        return p1.X == p2.X;
    }

    public int getX1()
    {
        return p1.X;
    }

    public int getX2()
    {
        return p2.X;
    }

    public int getY1()
    {
        return p1.Y;
    }

    public int getY2()
    {
        return p2.Y;
    }

    // XXX may need more precision
    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public int calculateFooting(int x)
    {
        if (p1.Y == p2.Y)
        {
            return p2.Y; // y at both ends is the same
        }

        //(y1 - y2) / (x1 - x2) = (y - y1) / (x - x1) ==> y = y1 + (x - x1) * (y1 - y2) / (x1 - x2)
        return (int)Math.Ceiling(p1.Y + (x - p1.X) * (double)(p1.Y - p2.Y) / (p1.X - p2.X));
    }

    public int CompareTo(Foothold? other)
    {
        if (other == null)
            return 1;

        if (p2.Y < other.getY1())
        {
            return -1;
        }
        else if (p1.Y > other.getY2())
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public int getId()
    {
        return id;
    }

    public int getNext()
    {
        return next;
    }

    public void setNext(int next)
    {
        this.next = next;
    }

    public int getPrev()
    {
        return prev;
    }

    public void setPrev(int prev)
    {
        this.prev = prev;
    }

    public bool Equals(Foothold? other)
    {
        if (other == null)
            return false;

        return other.id == id && other.p1 == p1 && other.p2 == p2 && other.prev == prev && other.next == next;
    }

    public override string ToString()
    {
        return $"Id {id}, LeftTop {p1}, RightBottom {p2}";
    }
}
