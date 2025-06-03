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

namespace net.server.services;


/**
 * @author Ronan
 */
public abstract class BaseScheduler
{
    private int idleProcs = 0;
    private List<Action<List<object>, bool>> listeners = new();
    private List<object> externalLocks = new();
    private Dictionary<object, KeyValuePair<AbstractRunnable, long>> registeredEntries = new();

    private Task? schedulerTask = null;
    private object schedulerLock = new object();

    CancellationTokenSource? cancellationTokenSource;

    WorldChannel _channelServer;
    protected BaseScheduler(WorldChannel worldChannel)
    {
        _channelServer = worldChannel;
    }

    // NOTE: practice EXTREME caution when adding external locks to the scheduler system, if you don't know what you're doing DON'T USE THIS.
    //protected BaseScheduler(List<object> extLocks)
    //{
    //    foreach (object lockObj in extLocks)
    //    {
    //        externalLocks.Add(lockObj);
    //    }
    //}

    protected void addListener(Action<List<object>, bool> listener)
    {
        listeners.Add(listener);
    }

    private void lockScheduler()
    {
        if (externalLocks.Count > 0)
        {
            foreach (object l in externalLocks)
            {
                Monitor.Enter(l);
            }
        }

        Monitor.Enter(schedulerLock);
    }

    private void unlockScheduler()
    {
        if (externalLocks.Count > 0)
        {
            foreach (object l in externalLocks)
            {
                Monitor.Exit(l);
            }
        }

        Monitor.Exit(schedulerLock);
    }

    private void runBaseSchedule()
    {
        List<object> toRemove;
        Dictionary<object, KeyValuePair<AbstractRunnable, long>> registeredEntriesCopy;

        lockScheduler();
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
        }
        finally
        {
            unlockScheduler();
        }

        long timeNow = _channelServer.getCurrentTime();
        toRemove = new();
        foreach (var rmd in registeredEntriesCopy)
        {
            KeyValuePair<AbstractRunnable, long> r = rmd.Value;

            if (r.Value < timeNow)
            {
                r.Key.run();  // runs the scheduled action
                toRemove.Add(rmd.Key);
            }
        }

        if (toRemove.Count > 0)
        {
            lockScheduler();
            try
            {
                foreach (object o in toRemove)
                {
                    registeredEntries.Remove(o);
                }
            }
            finally
            {
                unlockScheduler();
            }
        }

        dispatchRemovedEntries(toRemove, true);
    }

    protected void registerEntry(object key, AbstractRunnable removalAction, long duration)
    {
        lockScheduler();
        try
        {
            idleProcs = 0;
            if (schedulerTask == null)
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

            registeredEntries.AddOrUpdate(key, new(removalAction, _channelServer.getCurrentTime() + duration));
        }
        finally
        {
            unlockScheduler();
        }
    }

    protected void interruptEntry(object key)
    {
        AbstractRunnable? toRun = null;

        lockScheduler();
        try
        {
            if (registeredEntries.Remove(key, out var rm))
            {
                toRun = rm.Key;
            }
        }
        finally
        {
            unlockScheduler();
        }

        if (toRun != null)
        {
            toRun.run();
        }

        dispatchRemovedEntries(Collections.singletonList(key), false);
    }

    private void dispatchRemovedEntries(List<object> toRemove, bool fromUpdate)
    {
        foreach (var listener in listeners.ToArray())
        {
            listener.Invoke(toRemove, fromUpdate);
        }
    }

    public virtual void dispose()
    {
        lockScheduler();
        try
        {
            cancellationTokenSource?.Cancel();
            if (schedulerTask != null)
            {
                schedulerTask = null;
            }

            listeners.Clear();
            registeredEntries.Clear();
        }
        finally
        {
            unlockScheduler();
            externalLocks.Clear();
        }
    }
}
