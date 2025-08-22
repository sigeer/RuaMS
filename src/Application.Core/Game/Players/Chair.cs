using Application.Core.Channel.Services;
using Application.Core.Game.Skills;
using Application.Core.model;
using Microsoft.Extensions.DependencyInjection;
using server;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private ScheduledFuture? chairRecoveryTask = null;

        private AtomicInteger chair = new AtomicInteger(-1);

        private void unsitChairInternal()
        {
            int chairid = chair.get();
            if (chairid >= 0)
            {
                if (ItemConstants.isFishingChair(chairid))
                {
                    Client.CurrentServerContainer.ServiceProvider.GetRequiredService<IFishingService>().StopFishing(this);
                }

                setChair(-1);
                if (unregisterChairBuff())
                {
                    MapModel.broadcastMessage(this, PacketCreator.cancelForeignChairSkillEffect(this.getId()), false);
                }

                MapModel.broadcastMessage(this, PacketCreator.showChair(this.getId(), 0), false);
            }

            sendPacket(PacketCreator.cancelChair(-1));
        }

        public void sitChair(int itemId)
        {
            if (this.isLoggedinWorld())
            {
                if (itemId >= 1000000)
                {    // sit on item chair
                    if (chair.get() < 0)
                    {
                        setChair(itemId);
                        MapModel.broadcastMessage(this, PacketCreator.showChair(this.getId(), itemId), false);
                    }
                    sendPacket(PacketCreator.enableActions());
                }
                else if (itemId >= 0)
                {    // sit on map chair
                    if (chair.get() < 0)
                    {
                        setChair(itemId);
                        if (registerChairBuff())
                        {
                            MapModel.broadcastMessage(this, PacketCreator.giveForeignChairSkillEffect(this.getId()), false);
                        }
                        sendPacket(PacketCreator.cancelChair(itemId));
                    }
                }
                else
                {    // stand up
                    unsitChairInternal();
                }
            }
        }

        private void setChair(int chair)
        {
            this.chair.set(chair);
        }

        private void startChairTask()
        {
            if (chair.get() < 0)
            {
                return;
            }

            int healInterval;
            Monitor.Enter(effLock);
            try
            {
                updateChairHealStats();
                healInterval = localchairrate;
            }
            finally
            {
                Monitor.Exit(effLock);
            }

            chLock.EnterReadLock();
            try
            {
                if (chairRecoveryTask != null)
                {
                    stopChairTask();
                }

                chairRecoveryTask = Client.CurrentServerContainer.TimerManager.register(() =>
                {
                    updateChairHealStats();
                    int healHP = localchairhp;
                    int healMP = localchairmp;

                    if (HP < ActualMaxHP)
                    {
                        byte recHP = (byte)(healHP / YamlConfig.config.server.CHAIR_EXTRA_HEAL_MULTIPLIER);

                        sendPacket(PacketCreator.showOwnRecovery(recHP));
                        MapModel.broadcastMessage(this, PacketCreator.showRecovery(Id, recHP), false);
                    }

                    UpdateStatsChunk(() =>
                    {
                        ChangeHP(healHP);
                        ChangeMP(healMP);
                    });
                }, healInterval, healInterval);
            }
            finally
            {
                chLock.ExitReadLock();
            }
        }
        private void stopChairTask()
        {
            chLock.EnterReadLock();
            try
            {
                if (chairRecoveryTask != null)
                {
                    chairRecoveryTask.cancel(false);
                    chairRecoveryTask = null;
                }
            }
            finally
            {
                chLock.ExitReadLock();
            }
        }

        private static ChairHealStats getChairTaskIntervalRate(int maxhp, int maxmp)
        {
            float toHeal = Math.Max(maxhp, maxmp);
            float maxDuration = (float)TimeSpan.FromSeconds(YamlConfig.config.server.CHAIR_EXTRA_HEAL_MAX_DELAY).TotalMilliseconds;

            int rate = 0;
            int minRegen = 1, maxRegen = (256 * YamlConfig.config.server.CHAIR_EXTRA_HEAL_MULTIPLIER) - 1, midRegen = 1;
            while (minRegen < maxRegen)
            {
                midRegen = (int)((minRegen + maxRegen) * 0.94);

                float procsTemp = toHeal / midRegen;
                float newRate = maxDuration / procsTemp;
                rate = (int)newRate;

                if (newRate < 420)
                {
                    minRegen = (int)(1.2 * midRegen);
                }
                else if (newRate > 5000)
                {
                    maxRegen = (int)(0.8 * midRegen);
                }
                else
                {
                    break;
                }
            }

            float procs = maxDuration / rate;
            int hpRegen, mpRegen;
            if (maxhp > maxmp)
            {
                hpRegen = midRegen;
                mpRegen = (int)Math.Ceiling(maxmp / procs);
            }
            else
            {
                hpRegen = (int)Math.Ceiling(maxhp / procs);
                mpRegen = midRegen;
            }

            return new(rate, hpRegen, mpRegen);
        }

        private void updateChairHealStats()
        {
            statLock.EnterReadLock();
            try
            {
                if (localchairrate != -1)
                {
                    return;
                }
            }
            finally
            {
                statLock.ExitReadLock();
            }

            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                var p = getChairTaskIntervalRate(ActualMaxHP, ActualMaxMP);

                localchairrate = p.Rate;
                localchairhp = p.Hp;
                localchairmp = p.Mp;
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }
        }

        public bool unregisterChairBuff()
        {
            if (!YamlConfig.config.server.USE_CHAIR_EXTRAHEAL)
            {
                return false;
            }

            int skillId = JobModel.getJobMapChair();
            int skillLv = getSkillLevel(skillId);
            if (skillLv > 0)
            {
                StatEffect mapChairSkill = SkillFactory.getSkill(skillId)!.getEffect(skillLv);
                return cancelEffect(mapChairSkill, false, -1);
            }

            return false;
        }

        public bool registerChairBuff()
        {
            if (!YamlConfig.config.server.USE_CHAIR_EXTRAHEAL)
            {
                return false;
            }

            int skillId = JobModel.getJobMapChair();
            int skillLv = getSkillLevel(skillId);
            if (skillLv > 0)
            {
                StatEffect mapChairSkill = SkillFactory.getSkill(skillId)!.getEffect(skillLv);
                mapChairSkill.applyTo(this);
                return true;
            }

            return false;
        }
    }
}
