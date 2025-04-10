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
using Application.Core.Scripting.Infrastructure;
using System.Collections.Concurrent;

namespace scripting.Event;



/**
 * @author Matze
 */
public class EventScriptManager : AbstractScriptManager
{
    private const string INJECTED_VARIABLE_NAME = "em";
    private static EventEntry? fallback;
    private ConcurrentDictionary<string, EventEntry> events = new();
    private bool active = false;
    public IWorldChannel LinkedWorldChannel { get; }
    readonly string[] eventScripts;

    private class EventEntry
    {

        public EventEntry(IEngine iv, EventManager em)
        {
            this.iv = iv;
            this.em = em;
        }

        public IEngine iv;
        public EventManager em;
    }

    public EventScriptManager(IWorldChannel channel, string[] scripts)
    {
        LinkedWorldChannel = channel;
        eventScripts = scripts;
        foreach (string script in eventScripts)
        {
            if (!string.IsNullOrEmpty(script))
            {
                events.AddOrUpdate(GetEventName(script), initializeEventEntry(script));
            }
        }

        init();
        events.TryRemove("0_EXAMPLE", out fallback);
    }

    private string GetEventName(string eventScript)
    {
        return Path.GetFileNameWithoutExtension(eventScript);
    }

    public EventManager? getEventManager(string evt)
    {
        EventEntry? entry = events.GetValueOrDefault(evt);
        if (entry == null)
        {
            return fallback?.em;
        }
        return entry.em;
    }

    public bool isActive()
    {
        return active;
    }

    public void init()
    {
        foreach (EventEntry entry in events.Values)
        {
            try
            {
                entry.iv.CallFunction("init");
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error on script: {ScriptName}", entry.em.getName());
            }
        }

        active = events.Count > 1; // bootup loads only 1 script
    }

    private void reloadScripts()
    {
        foreach (string script in eventScripts)
        {
            if (!string.IsNullOrEmpty(script))
            {
                events.AddOrUpdate(GetEventName(script), initializeEventEntry(script));
            }
        }
    }

    private EventEntry initializeEventEntry(string script)
    {
        var engine = getInvocableScriptEngine(GetEventScriptPath(script));
        EventManager eventManager = new EventManager(LinkedWorldChannel, engine, script);
        engine.AddHostedObject(INJECTED_VARIABLE_NAME, eventManager);
        return new EventEntry(engine, eventManager);
    }

    // Is never being called
    public void reload()
    {
        cancel();
        reloadScripts();
        init();
    }

    public void cancel()
    {
        active = false;
        foreach (EventEntry entry in events.Values)
        {
            entry.em.cancel();
        }
    }

    public void dispose()
    {
        if (events.Count == 0)
        {
            return;
        }

        HashSet<EventEntry> eventEntries = events.Values.ToHashSet();
        events.Clear();

        active = false;
        foreach (EventEntry entry in eventEntries)
        {
            entry.em.cancel();
        }
    }
}
