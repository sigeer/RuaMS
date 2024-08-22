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


using Application.Core.scripting.Event;
using client;
using tools;

namespace server.maps;

/**
 * @author Ronan
 */
public class MiniDungeon
{
    List<Character> players = new();
    ScheduledFuture? timeoutTask = null;
    object lockObj = new object();

    int baseMap;
    long expireTime;

    public MiniDungeon(int baseValue, long timeLimit)
    {
        baseMap = baseValue;
        expireTime = timeLimit * 1000;

        timeoutTask = TimerManager.getInstance().schedule(() => close(), expireTime);
        expireTime += DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    public bool registerPlayer(Character chr)
    {
        int time = (int)((expireTime - DateTimeOffset.Now.ToUnixTimeMilliseconds()) / 1000);
        if (time > 0)
        {
            chr.sendPacket(PacketCreator.getClock(time));
        }

        Monitor.Enter(lockObj);
        try
        {
            if (timeoutTask == null)
            {
                return false;
            }

            players.Add(chr);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }

        return true;
    }

    public bool unregisterPlayer(Character chr)
    {
        chr.sendPacket(PacketCreator.removeClock());

        Monitor.Enter(lockObj);
        try
        {
            players.Remove(chr);

            if (players.Count == 0)
            {
                dispose();
                return false;
            }
        }
        finally
        {
            Monitor.Exit(lockObj);
        }

        if (chr.isPartyLeader())
        {  // thanks Conrad for noticing party is not sent out of the MD as soon as leader leaves it
            close();
        }

        return true;
    }

    public void close()
    {
        Monitor.Enter(lockObj);
        try
        {
            List<Character> lchr = new(players);

            foreach (Character chr in lchr)
            {
                chr.changeMap(baseMap);
            }

            dispose();
            timeoutTask = null;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void dispose()
    {
        Monitor.Enter(lockObj);
        try
        {
            players.Clear();

            if (timeoutTask != null)
            {
                timeoutTask.cancel(false);
                timeoutTask = null;
            }
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }
}
