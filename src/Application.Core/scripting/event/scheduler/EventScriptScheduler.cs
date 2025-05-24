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


using Application.Core.Game.TheWorld;

namespace scripting.Event.scheduler;
/**
 * @author Ronan
 */
public class EventScriptScheduler
{

    private bool disposed = false;
    private int idleProcs = 0;
    private Dictionary<Action, long> registeredEntries = new();

    private Task? schedulerTask = null;
    private object schedulerLock = new object();

    CancellationTokenSource? cancellationTokenSource;

    readonly IWorldChannel _channelServer;

    public EventScriptScheduler(IWorldChannel channelServer)
    {
        _channelServer = channelServer;
    }

    private void runBaseSchedule()
    {
        List<Action> toRemove;
        Dictionary<Action, long> registeredEntriesCopy;

        Monitor.Enter(schedulerLock);
        try
        {
            if (registeredEntries.Count == 0)
            {
                idleProcs++;

                if (idleProcs >= YamlConfig.config.server.MOB_STATUS_MONITOR_LIFE)
                {
                    if (schedulerTask != null)
                    {
                        cancellationTokenSource?.Cancel();
                        schedulerTask = null;
                    }
                }

                return;
            }

            idleProcs = 0;
            registeredEntriesCopy = new(registeredEntries);


            long timeNow = _channelServer.getCurrentTime();
            toRemove = new();
            foreach (var rmd in registeredEntriesCopy)
            {
                if (rmd.Value < timeNow)
                {
                    var r = rmd.Key;

                    r.Invoke();  // runs the scheduled action
                    toRemove.Add(r);
                }
            }

            if (toRemove.Count > 0)
            {
                foreach (Action r in toRemove)
                {
                    registeredEntries.Remove(r);
                }
            }
        }
        finally
        {
            Monitor.Exit(schedulerLock);
        }
    }

    public void registerEntry(Action scheduledAction, long duration)
    {
        Monitor.Enter(schedulerLock);
        try
        {
            idleProcs = 0;
            if (schedulerTask == null && !disposed)
            {
                cancellationTokenSource = new CancellationTokenSource();
                schedulerTask = Task.Run(async () =>
                {
                    while (!cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        await Task.Delay(YamlConfig.config.server.MOB_STATUS_MONITOR_PROC);
                        runBaseSchedule();
                    }
                }, cancellationTokenSource.Token);
            }

            registeredEntries.Add(scheduledAction, _channelServer.getCurrentTime() + duration);
        }
        finally
        {
            Monitor.Exit(schedulerLock);
        }
    }

    public void cancelEntry(Action scheduledAction)
    {
        Monitor.Enter(schedulerLock);
        try
        {
            registeredEntries.Remove(scheduledAction);
        }
        finally
        {
            Monitor.Exit(schedulerLock);
        }
    }

    public void dispose()
    {
        Monitor.Enter(schedulerLock);
        try
        {
            cancellationTokenSource?.Cancel();
            if (schedulerTask != null)
            {
                schedulerTask = null;
            }

            registeredEntries.Clear();
            disposed = true;
        }
        finally
        {
            Monitor.Exit(schedulerLock);
        }
    }
}
