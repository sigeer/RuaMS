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

namespace net.server.services.task.channel;






/**
 * @author Ronan
 */
public class MobAnimationService : BaseService
{

    private MobAnimationScheduler[] mobAnimationSchedulers = new MobAnimationScheduler[YamlConfig.config.server.CHANNEL_LOCKS];

    public MobAnimationService()
    {
        for (int i = 0; i < YamlConfig.config.server.CHANNEL_LOCKS; i++)
        {
            mobAnimationSchedulers[i] = new MobAnimationScheduler();
        }
    }

    public override void dispose()
    {
        for (int i = 0; i < YamlConfig.config.server.CHANNEL_LOCKS; i++)
        {
            if (mobAnimationSchedulers[i] != null)
            {
                mobAnimationSchedulers[i].dispose();
                mobAnimationSchedulers[i] = null;
            }
        }
    }

    public bool registerMobOnAnimationEffect(int mapid, int mobHash, long delay)
    {
        return mobAnimationSchedulers[getChannelSchedulerIndex(mapid)].registerAnimationMode(mobHash, delay);
    }

    private static EmptyRunnable r = new EmptyRunnable();

    private class MobAnimationScheduler : BaseScheduler
    {
        HashSet<int> onAnimationMobs = new(1000);
        private object animationLock = new object();

        public MobAnimationScheduler()
        {
            base.addListener((toRemove, update) =>
            {
                Monitor.Enter(animationLock);
                try
                {
                    foreach (object hashObj in toRemove)
                    {
                        int mobHash = (int)hashObj;
                        onAnimationMobs.Remove(mobHash);
                    }
                }
                finally
                {
                    Monitor.Exit(animationLock);
                }
            });
        }

        public bool registerAnimationMode(int mobHash, long animationTime)
        {
            Monitor.Enter(animationLock);
            try
            {
                if (onAnimationMobs.Contains(mobHash))
                {
                    return false;
                }

                registerEntry(mobHash, r, animationTime);
                onAnimationMobs.Add(mobHash);
                return true;
            }
            finally
            {
                Monitor.Exit(animationLock);
            }
        }

        public override void dispose()
        {

        }

    }

}
