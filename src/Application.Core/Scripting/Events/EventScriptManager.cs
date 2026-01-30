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
using Application.Core.Game.Commands;
using Application.Core.Scripting.Events;
using Application.Shared.Events;
using Microsoft.Extensions.Logging;
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
    readonly string[] eventScripts;

    public EventScriptManager(WorldChannel channel, ILogger<AbstractScriptManager> logger, CommandExecutor commandExecutor, IEnumerable<IAddtionalRegistry> addtionalRegistries)
        : base(logger, commandExecutor, channel, addtionalRegistries)
    {
        eventScripts = ScriptSource.Instance.GetEvents();
    }

    public int ReloadEventScript()
    {
        DisposeEvents();

        foreach (string script in eventScripts)
        {
            if (!string.IsNullOrEmpty(script))
            {
                var em = initializeEventEntry(script);

                try
                {
                    em.getIv().CallFunction("init").ToString();
                }
                catch (Exception ex)
                {
                    throw new BusinessFatalException($"事件脚本{script}加载失败：" + ex.Message);
                }

                if (!events.TryAdd(em.getName(), em))
                {
                    throw new BusinessFatalException($"事件名重复，名称：{em.getName()}");
                }
            }
        }
        active = events.Count > 0;
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
        return active;
    }


    private EventManager initializeEventEntry(string script)
    {
        var scriptFile = GetEventScriptPath(script);
        var engine = getInvocableScriptEngine(scriptFile);

        EventManager em;
        var evenTypeStr = engine.GetValue("eventType").ToObject<string>();
        if (Enum.TryParse<EventType>(evenTypeStr, true, out var type))
        {
            switch (type)
            {
                case EventType.PartyQuest:
                    em = new PartyQuestEventManager(_channelServer, engine, scriptFile);
                    break;
                case EventType.Expedition:
                    em = new ExpeditionEventManager(_channelServer, engine, scriptFile);
                    break;
                case EventType.Solo:
                    em = new SoloEventManager(_channelServer, engine, scriptFile);
                    break;
                case EventType.GuildQuest:
                    em = new GuildQuestEventManager(_channelServer, engine, scriptFile);
                    break;
                case EventType.CPQ:
                    em = new MonsterCarnivalEventManager(_channelServer, engine, scriptFile);
                    break;
                default:
                    em = new EventManager(_channelServer, engine, scriptFile);
                    break;
            }
        }
        else
        {
            em = new EventManager(_channelServer, engine, scriptFile);
        }
        engine.AddHostedObject(INJECTED_VARIABLE_NAME, em);
        return em;
    }


    public void cancel()
    {
        active = false;
        foreach (var entry in events.Values)
        {
            entry.Dispose();
        }
    }

    void DisposeEvents()
    {
        var cleanEvents = events.Values.ToList();
        foreach (var old in cleanEvents)
        {
            old.Dispose();
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
