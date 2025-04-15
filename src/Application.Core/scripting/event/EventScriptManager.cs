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


using Application.Core.Game.TheWorld;
using System.Collections.Concurrent;

namespace scripting.Event;



/**
 * @author Matze
 */
public class EventScriptManager : AbstractScriptManager
{
    private const string INJECTED_VARIABLE_NAME = "em";

    private ConcurrentDictionary<string, EventManager> events = new();
    private bool active = false;
    public IWorldChannel LinkedWorldChannel { get; }
    readonly string[] eventScripts;


    public EventScriptManager(IWorldChannel channel, string[] scripts)
    {
        LinkedWorldChannel = channel;
        eventScripts = scripts;

        ReloadEventScript();
    }

    public void ReloadEventScript()
    {
        DisposeEvents();

        foreach (string script in eventScripts)
        {
            if (!string.IsNullOrEmpty(script))
            {
                var em = initializeEventEntry(script);

                var eventName = em.getIv().CallFunction("init").ToString();
                if (string.IsNullOrEmpty(eventName))
                {
                    eventName = GetEventName(script);
                }
                if (!events.TryAdd(eventName, em))
                {
                    throw new BusinessFatalException($"事件名重复，名称：{eventName}");
                }
            }
        }
        active = events.Count > 0;
    }

    private string GetEventName(string eventScript)
    {
        return Path.GetFileNameWithoutExtension(eventScript);
    }


    public EventManager? getEventManager(string evt)
    {
        return events.GetValueOrDefault(evt);
    }

    public bool isActive()
    {
        return active;
    }


    private EventManager initializeEventEntry(string script)
    {
        var engine = getInvocableScriptEngine(GetEventScriptPath(script));
        EventManager eventManager = new EventManager(LinkedWorldChannel, engine, script);
        engine.AddHostedObject(INJECTED_VARIABLE_NAME, eventManager);
        return eventManager;
    }


    public void cancel()
    {
        active = false;
        foreach (var entry in events.Values)
        {
            entry.cancel();
        }
    }

    void DisposeEvents()
    {
        var cleanEvents = events.Values.ToList();
        foreach (var old in cleanEvents)
        {
            old.cancel();
        }
        events.Clear();
    }

    public void dispose()
    {
        if (events.Count == 0)
        {
            return;
        }

        DisposeEvents();

        active = false;
    }
}
