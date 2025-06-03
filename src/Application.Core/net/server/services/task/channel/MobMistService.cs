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
public class MobMistService : BaseService
{

    private MobMistScheduler[] mobMistSchedulers = new MobMistScheduler[YamlConfig.config.server.CHANNEL_LOCKS];

    public MobMistService(WorldChannel worldChannel) : base(worldChannel)
    {
        for (int i = 0; i < YamlConfig.config.server.CHANNEL_LOCKS; i++)
        {
            mobMistSchedulers[i] = new MobMistScheduler(worldChannel);
        }
    }

    public override void dispose()
    {
        for (int i = 0; i < YamlConfig.config.server.CHANNEL_LOCKS; i++)
        {
            if (mobMistSchedulers[i] != null)
            {
                mobMistSchedulers[i].dispose();
            }
        }
    }

    public void registerMobMistCancelAction(int mapid, Action runAction, long delay)
    {
        mobMistSchedulers[getChannelSchedulerIndex(mapid)].registerMistCancelAction(runAction, delay);
    }

    private class MobMistScheduler : BaseScheduler
    {
        public MobMistScheduler(WorldChannel worldChannel) : base(worldChannel)
        {
        }

        public void registerMistCancelAction(Action runAction, long delay)
        {
            registerEntry(runAction, TempRunnable.Parse(runAction), delay);
        }

    }

}
