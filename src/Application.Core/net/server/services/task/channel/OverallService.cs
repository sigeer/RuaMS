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

namespace net.server.services.task.channel;

/**
 * @author Ronan
 */
public class OverallService : BaseService
{   // thanks Alex for suggesting a refactor over the several channel schedulers unnecessarily populating the Channel class

    private OverallScheduler[] channelSchedulers = new OverallScheduler[YamlConfig.config.server.CHANNEL_LOCKS];

    public OverallService(WorldChannel worldChannel) : base(worldChannel)
    {
        for (int i = 0; i < YamlConfig.config.server.CHANNEL_LOCKS; i++)
        {
            channelSchedulers[i] = new OverallScheduler(worldChannel);
        }
    }

    public override void dispose()
    {
        for (int i = 0; i < YamlConfig.config.server.CHANNEL_LOCKS; i++)
        {
            if (channelSchedulers[i] != null)
            {
                channelSchedulers[i].dispose();
                channelSchedulers[i] = null;
            }
        }
    }

    public void registerOverallAction(int mapid, AbstractRunnable runAction, long delay)
    {
        channelSchedulers[getChannelSchedulerIndex(mapid)].registerDelayedAction(runAction, delay);
    }

    public void registerOverallAction(int mapid, Action runAction, long delay)
    {
        channelSchedulers[getChannelSchedulerIndex(mapid)].registerDelayedAction(TempRunnable.Parse(runAction), delay);
    }

    public void forceRunOverallAction(int mapid, AbstractRunnable runAction)
    {
        channelSchedulers[getChannelSchedulerIndex(mapid)].forceRunDelayedAction(runAction);
    }

    public void forceRunOverallAction(int mapid, Action runAction)
    {
        channelSchedulers[getChannelSchedulerIndex(mapid)].forceRunDelayedAction(TempRunnable.Parse(runAction));
    }


    public class OverallScheduler : BaseScheduler
    {
        public OverallScheduler(WorldChannel worldChannel) : base(worldChannel)
        {
        }

        public void registerDelayedAction(AbstractRunnable runAction, long delay)
        {
            registerEntry(runAction, runAction, delay);
        }

        public void forceRunDelayedAction(AbstractRunnable runAction)
        {
            interruptEntry(runAction);
        }

    }

}
