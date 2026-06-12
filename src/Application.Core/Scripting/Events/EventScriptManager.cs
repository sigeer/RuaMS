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


using Application.Core.Channel;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.Scripting.Events;
using Application.Utility.Tickables;
using System.Collections.Concurrent;

namespace scripting.Event;

/**
 * @author Matze
 */
public class EventScriptManager : ITickableTree, IDisposable
{
    private ConcurrentDictionary<string, EventManager> events = new();
    public bool IsActive => events.Count > 0;
    public int EventCount => events.Count;
    public WorldChannel ChannelServer { get; }

    public EventScriptManager(WorldChannel channel)
    {
        ChannelServer = channel;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="emList"></param>
    /// <returns></returns>
    /// <exception cref="BusinessFatalException"></exception>
    public int ReloadEventScript(List<EventManager> emList)
    {
        var duplicatedItem = emList.GroupBy(x => x.getName()).FirstOrDefault(x => x.Count() > 1);
        if (duplicatedItem != null)
        {
            throw new BusinessFatalException($"事件名重复，名称：{duplicatedItem.Key}");
        }

        foreach (var em in emList)
        {
            try
            {
                em.Initialize();

                if (events.TryGetValue(em.getName(), out var old))
                {
                    em.Inherit(old);
                }
                events[em.getName()] = em;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.ToString());
            }
        }
        return events.Count;
    }

    public EventManager? getEventManager(string evt)
    {
        return events.GetValueOrDefault(evt);
    }

    public int GetEventMaps()
    {
        return events.Values.OfType<AbstractInstancedEventManager>().Sum(x => x.getInstances().Sum(x => x.getMapFactory().getMaps().Count));
    }

    public int GetEventInstanceCount()
    {
        return events.Values.OfType<AbstractInstancedEventManager>().Sum(x => x.getInstances().Count);
    }

    public bool isActive()
    {
        return IsActive;
    }


    public void Dispose()
    {
        if (events.Count == 0)
        {
            return;
        }

        var cleanEvents = events.Values.ToList();
        events.Clear();
        foreach (var old in cleanEvents)
        {
            old.Dispose();
        }
    }

    public TickableStatus Status { get; protected set; }

    public List<ITickable> SubTickables => events.Values.OfType<ITickable>().ToList();

    public void OnTick(long now)
    {
        this.ProcessSubTickables(now);
    }
}
