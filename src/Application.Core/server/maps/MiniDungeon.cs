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


using Application.Core.Channel;
using tools;

namespace server.maps;

/**
 * @author Ronan
 */
public class MiniDungeon
{
    List<Player> players = new();
    ScheduledFuture? timeoutTask = null;
    Lock lockObj = new ();

    int baseMap;
    long expireTime;
    readonly WorldChannel _worldChannel;

    public MiniDungeon(WorldChannel worldChannel, int baseValue, long timeLimit)
    {
        _worldChannel = worldChannel;
        baseMap = baseValue;
        expireTime = timeLimit * 1000;

        timeoutTask = worldChannel.Container.TimerManager.schedule(() => close(), expireTime);
        expireTime += worldChannel.Container.getCurrentTime();
    }

    public bool registerPlayer(Player chr)
    {
        int time = (int)((expireTime - _worldChannel.Container.getCurrentTime()) / 1000);
        if (time > 0)
        {
            chr.sendPacket(PacketCreator.getClock(time));
        }

        lockObj.Enter();
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
            lockObj.Exit();
        }

        return true;
    }

    public bool unregisterPlayer(Player chr)
    {
        chr.sendPacket(PacketCreator.removeClock());

        lockObj.Enter();
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
            lockObj.Exit();
        }

        if (chr.isPartyLeader())
        {  // thanks Conrad for noticing party is not sent out of the MD as soon as leader leaves it
            close();
        }

        return true;
    }

    public void close()
    {
        lockObj.Enter();
        try
        {
            List<Player> lchr = new(players);

            foreach (Player chr in lchr)
            {
                chr.changeMap(baseMap);
            }

            dispose();
            timeoutTask = null;
        }
        finally
        {
            lockObj.Exit();
        }
    }

    public void dispose()
    {
        lockObj.Enter();
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
            lockObj.Exit();
        }
    }
}
