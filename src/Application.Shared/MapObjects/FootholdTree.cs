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
public class FootholdTree
{
    private FootholdTree? nw = null;
    private FootholdTree? ne = null;
    private FootholdTree? sw = null;
    private FootholdTree? se = null;
    private List<Foothold> footholds = new();
    private Point p1;
    private Point p2;
    private Point center;
    private int depth = 0;
    private static int maxDepth = 8;
    private int maxDropX;
    private int minDropX;

    public FootholdTree(Point p1, Point p2)
    {
        this.p1 = p1;
        this.p2 = p2;
        center = new Point((p2.X - p1.X) / 2, (p2.Y - p1.Y) / 2);
    }

    public FootholdTree(Point p1, Point p2, int depth)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.depth = depth;
        center = new Point((p2.X - p1.X) / 2, (p2.Y - p1.Y) / 2);
    }

    public void insert(Foothold f)
    {
        if (depth == 0)
        {
            if (f.getX1() > maxDropX)
            {
                maxDropX = f.getX1();
            }
            if (f.getX1() < minDropX)
            {
                minDropX = f.getX1();
            }
            if (f.getX2() > maxDropX)
            {
                maxDropX = f.getX2();
            }
            if (f.getX2() < minDropX)
            {
                minDropX = f.getX2();
            }
        }
        if (depth == maxDepth ||
                (f.getX1() >= p1.X && f.getX2() <= p2.X &&
                        f.getY1() >= p1.Y && f.getY2() <= p2.Y))
        {
            footholds.Add(f);
        }
        else
        {
            if (nw == null)
            {
                nw = new FootholdTree(p1, center, depth + 1);
                ne = new FootholdTree(new Point(center.X, p1.Y), new Point(p2.X, center.Y), depth + 1);
                sw = new FootholdTree(new Point(p1.X, center.Y), new Point(center.X, p2.Y), depth + 1);
                se = new FootholdTree(center, p2, depth + 1);
            }
            if (f.getX2() <= center.X && f.getY2() <= center.Y)
            {
                nw.insert(f);
            }
            else if (f.getX1() > center.X && f.getY2() <= center.Y)
            {
                ne!.insert(f);
            }
            else if (f.getX2() <= center.X && f.getY1() > center.Y)
            {
                sw!.insert(f);
            }
            else
            {
                se!.insert(f);
            }
        }
    }

    private List<Foothold> getRelevants(Point p)
    {
        return getRelevants(p, new());
    }

    private List<Foothold> getRelevants(Point p, List<Foothold> list)
    {
        list.AddRange(footholds);
        if (nw != null)
        {
            if (p.X <= center.X && p.Y <= center.Y)
            {
                nw.getRelevants(p, list);
            }
            else if (p.X > center.X && p.Y <= center.Y)
            {
                ne!.getRelevants(p, list);
            }
            else if (p.X <= center.X && p.Y > center.Y)
            {
                sw!.getRelevants(p, list);
            }
            else
            {
                se!.getRelevants(p, list);
            }
        }
        return list;
    }

    private Foothold? findWallR(Point p1, Point p2)
    {
        Foothold? ret;
        foreach (Foothold f in footholds)
        {
            if (f.isWall() && f.getX1() >= p1.X && f.getX1() <= p2.X &&
                    f.getY1() >= p1.Y && f.getY2() <= p1.Y)
            {
                return f;
            }
        }
        if (nw != null)
        {
            if (p1.X <= center.X && p1.Y <= center.Y)
            {
                ret = nw.findWallR(p1, p2);
                if (ret != null)
                {
                    return ret;
                }
            }
            if ((p1.X > center.X || p2.X > center.X) && p1.Y <= center.Y)
            {
                ret = ne!.findWallR(p1, p2);
                if (ret != null)
                {
                    return ret;
                }
            }
            if (p1.X <= center.X && p1.Y > center.Y)
            {
                ret = sw!.findWallR(p1, p2);
                if (ret != null)
                {
                    return ret;
                }
            }
            if ((p1.X > center.X || p2.X > center.X) && p1.Y > center.Y)
            {
                ret = se!.findWallR(p1, p2);
                return ret;
            }
        }
        return null;
    }

    public Foothold? findWall(Point p1, Point p2)
    {
        if (p1.Y != p2.Y)
        {
            throw new ArgumentException();
        }
        return findWallR(p1, p2);
    }

    public Foothold? findBelow(Point p)
    {
        List<Foothold> relevants = getRelevants(p);
        List<Foothold> xMatches = new();
        foreach (Foothold fh in relevants)
        {
            if (fh.getX1() <= p.X && fh.getX2() >= p.X)
            {
                xMatches.Add(fh);
            }
        }
        xMatches.Sort();
        foreach (Foothold fh in xMatches)
        {
            if (!fh.isWall())
            {
                if (fh.getY1() != fh.getY2())
                {
                    int calcY;
                    double s1 = Math.Abs(fh.getY2() - fh.getY1());
                    double s2 = Math.Abs(fh.getX2() - fh.getX1());
                    double s4 = Math.Abs(p.X - fh.getX1());
                    double alpha = Math.Atan(s2 / s1);
                    double beta = Math.Atan(s1 / s2);
                    double s5 = Math.Cos(alpha) * (s4 / Math.Cos(beta));
                    if (fh.getY2() < fh.getY1())
                    {
                        calcY = fh.getY1() - (int)s5;
                    }
                    else
                    {
                        calcY = fh.getY1() + (int)s5;
                    }
                    if (calcY >= p.Y)
                    {
                        return fh;
                    }
                }
                else
                {
                    if (fh.getY1() >= p.Y)
                    {
                        return fh;
                    }
                }
            }
        }
        return null;
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

    public int getMaxDropX()
    {
        return maxDropX;
    }

    public int getMinDropX()
    {
        return minDropX;
    }
}
