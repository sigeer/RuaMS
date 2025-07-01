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

using Application.Core.Game.Controllers;
using scripting.Event;
using System.Collections.Concurrent;

namespace Application.Core.Channel;

/**
 * @author Ronan
 */
public class EventRecallManager : TimelyControllerBase
{

    private ConcurrentDictionary<int, EventInstanceManager> eventHistory = new();

    public EventRecallManager(WorldChannel worldChannel) 
        : base($"EventRecallController_{worldChannel.ServerName}", TimeSpan.FromHours(1), TimeSpan.FromHours(1))
    {
    }

    protected override void HandleRun()
    {
        if (eventHistory.Count > 0)
        {
            List<int> toRemove = new();

            foreach (var eh in eventHistory)
            {
                if (!isRecallableEvent(eh.Value))
                {
                    toRemove.Add(eh.Key);
                }
            }

            foreach (int r in toRemove)
            {
                eventHistory.Remove(r);
            }
        }
    }


    private bool isRecallableEvent(EventInstanceManager? eim)
    {
        return eim != null && !eim.isEventDisposed() && !eim.isEventCleared();
    }

    public EventInstanceManager? recallEventInstance(int characterId)
    {
        if (eventHistory.TryRemove(characterId, out var eim))
            return isRecallableEvent(eim) ? eim : null;
        return null;
    }

    public void storeEventInstance(int characterId, EventInstanceManager eim)
    {
        if (YamlConfig.config.server.USE_ENABLE_RECALL_EVENT && isRecallableEvent(eim))
        {
            eventHistory.AddOrUpdate(characterId, eim);
        }
    }
}
