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
using Application.Core.Channel.Commands;
using Application.Core.Game.Life.Monsters;

namespace net.server.services.task.channel;

/**
 *
 * @author Ronan
 */
public class MobStatusService : BaseService
{

    private MobStatusScheduler[] mobStatusSchedulers = new MobStatusScheduler[YamlConfig.config.server.CHANNEL_LOCKS];

    public MobStatusService(WorldChannel worldChannel) : base(worldChannel)
    {
        for (int i = 0; i < YamlConfig.config.server.CHANNEL_LOCKS; i++)
        {
            mobStatusSchedulers[i] = new MobStatusScheduler(worldChannel);
        }
    }

    public override void dispose()
    {
        for (int i = 0; i < YamlConfig.config.server.CHANNEL_LOCKS; i++)
        {
            if (mobStatusSchedulers[i] != null)
            {
                mobStatusSchedulers[i].dispose();
            }
        }
    }

    public void registerMobStatus(int mapid, MonsterStatusEffect mse, IWorldChannelCommand cancelAction, long duration)
    {
        registerMobStatus(mapid, mse, cancelAction, duration, null, -1);
    }

    public void registerMobStatus(int mapid, MonsterStatusEffect mse, IWorldChannelCommand cancelAction, long duration, IWorldChannelCommand? overtimeAction, int overtimeDelay)
    {
        mobStatusSchedulers[getChannelSchedulerIndex(mapid)].registerMobStatus(mse, cancelAction, duration, overtimeAction, overtimeDelay);
    }

    public void interruptMobStatus(int mapid, MonsterStatusEffect mse)
    {
        mobStatusSchedulers[getChannelSchedulerIndex(mapid)].interruptMobStatus(mse);
    }

    public class MobStatusScheduler : BaseScheduler
    {

        private Dictionary<MonsterStatusEffect, MobStatusOvertimeEntry> registeredMobStatusOvertime = new();

        public class MobStatusOvertimeEntry
        {
            private int procCount;
            private int procLimit;
            private IWorldChannelCommand r;

            public MobStatusOvertimeEntry(int delay, IWorldChannelCommand run)
            {
                procCount = 0;
                procLimit = (int)Math.Ceiling((float)delay / YamlConfig.config.server.MOB_STATUS_MONITOR_PROC);
                r = run;
            }

            public void update(List<IWorldChannelCommand> toRun)
            {
                procCount++;
                if (procCount >= procLimit)
                {
                    procCount = 0;
                    toRun.Add(r);
                }
            }
        }

        public MobStatusScheduler(WorldChannel worldChannel) : base(worldChannel)
        {

            base.addListener((List<object> toRemove, bool update) =>
                {
                    List<IWorldChannelCommand> toRun = new();

                    foreach (object mseo in toRemove)
                    {
                        MonsterStatusEffect mse = (MonsterStatusEffect)mseo;
                        registeredMobStatusOvertime.Remove(mse);
                    }

                    if (update)
                    {
                        // it's probably ok to use one thread for both management & overtime actions
                        List<MobStatusOvertimeEntry> mdoeList = new(registeredMobStatusOvertime.Values);
                        foreach (MobStatusOvertimeEntry mdoe in mdoeList)
                        {
                            mdoe.update(toRun);
                        }
                    }


                    foreach (var r in toRun)
                    {
                        worldChannel.Post(r);
                    }
                }
            );
        }

        public void registerMobStatus(MonsterStatusEffect mse, IWorldChannelCommand cancelStatus, long duration, IWorldChannelCommand? overtimeStatus, int overtimeDelay)
        {
            if (overtimeStatus != null)
            {
                MobStatusOvertimeEntry mdoe = new MobStatusOvertimeEntry(overtimeDelay, overtimeStatus);

                registeredMobStatusOvertime.AddOrUpdate(mse, mdoe);
            }

            registerEntry(mse, cancelStatus, duration);
        }

        public void interruptMobStatus(MonsterStatusEffect mse)
        {
            interruptEntry(mse);
        }

        public override void dispose()
        {
            base.dispose();
        }
    }

}
