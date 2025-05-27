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

namespace tools;

/**
 *
 * @author Ronan
 */
public class IntervalBuilder
{

    private List<Line2D> intervalLimits = new();

    protected ReaderWriterLockSlim intervalLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    public IntervalBuilder()
    {
    }

    private void refitOverlappedIntervals(int st, int en, int newFrom, int newTo)
    {
        List<Line2D> checkLimits = new(intervalLimits.Skip(st).Take(en));

        float newLimitX1, newLimitX2;
        if (checkLimits.Count > 0)
        {
            Line2D firstLimit = checkLimits[0];
            Line2D lastLimit = checkLimits.get(checkLimits.Count - 1);

            newLimitX1 = (newFrom < firstLimit.getX1()) ? newFrom : firstLimit.getX1();
            newLimitX2 = (newTo > lastLimit.getX2()) ? newTo : lastLimit.getX2();

            foreach (Line2D limit in checkLimits)
            {
                intervalLimits.Remove(limit);
            }
        }
        else
        {
            newLimitX1 = newFrom;
            newLimitX2 = newTo;
        }

        intervalLimits.Insert(st, Line2D.Float((float)newLimitX1, 0, (float)newLimitX2, 0));
    }

    private int bsearchInterval(int point)
    {
        int st = 0, en = intervalLimits.Count - 1;

        int mid, idx;
        while (en >= st)
        {
            idx = (st + en) / 2;
            mid = intervalLimits[idx].getX1();

            if (mid == point)
            {
                return idx;
            }
            else if (mid < point)
            {
                st = idx + 1;
            }
            else
            {
                en = idx - 1;
            }
        }

        return en;
    }

    public void addInterval(int from, int to)
    {
        intervalLock.EnterWriteLock();
        try
        {
            int st = bsearchInterval(from);
            if (st < 0)
            {
                st = 0;
            }
            else if (intervalLimits[st].getX2() < from)
            {
                st += 1;
            }

            int en = bsearchInterval(to);
            if (en < st) en = st - 1;

            refitOverlappedIntervals(st, en + 1, from, to);
        }
        finally
        {
            intervalLock.ExitWriteLock();
        }
    }

    public bool inInterval(int point)
    {
        return inInterval(point, point);
    }

    public bool inInterval(int from, int to)
    {
        intervalLock.EnterReadLock();
        try
        {
            int idx = bsearchInterval(from);
            return idx >= 0 && to <= intervalLimits[idx].getX2();
        }
        finally
        {
            intervalLock.ExitReadLock();
        }
    }

    public void clear()
    {
        intervalLock.EnterWriteLock();
        try
        {
            intervalLimits.Clear();
        }
        finally
        {
            intervalLock.ExitWriteLock();
        }
    }

}
