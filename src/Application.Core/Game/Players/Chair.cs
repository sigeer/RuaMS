using Application.Core.Game.Skills;
using Application.Core.model;
using server;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private AtomicInteger chair = new AtomicInteger(-1);

        private async Task unsitChairInternal()
        {
            int chairid = chair.get();
            if (chairid >= 0)
            {
                if (ItemConstants.isFishingChair(chairid))
                {
                    Client.CurrentServer.NodeService.FishingService.StopFishing(this);
                }

                setChair(-1);
                if (await unregisterChairBuff())
                {
                    await BroadcastMap(PacketCreator.cancelForeignChairSkillEffect(this.getId()), Id);
                }

                await BroadcastMap(PacketCreator.showChair(this.getId(), 0), Id);
            }

            await SendPacket(PacketCreator.cancelChair(-1));
        }

        public async Task sitChair(int itemId)
        {
            if (this.isLoggedinWorld())
            {
                if (itemId >= 1000000)
                {    // sit on item chair
                    if (chair.get() < 0)
                    {
                        setChair(itemId);
                        await BroadcastMap(PacketCreator.showChair(this.getId(), itemId), Id);
                    }
                    await SendPacket(PacketCreator.enableActions());
                }
                else if (itemId >= 0)
                {    // sit on map chair
                    if (chair.get() < 0)
                    {
                        setChair(itemId);
                        if (await registerChairBuff())
                        {
                            await BroadcastMap(PacketCreator.giveForeignChairSkillEffect(this.getId()), Id);
                        }
                        await SendPacket(PacketCreator.cancelChair(itemId));
                    }
                }
                else
                {    // stand up
                    await unsitChairInternal();
                }
            }
        }

        private void setChair(int chair)
        {
            this.chair.set(chair);
        }

        public static ChairHealStats getChairTaskIntervalRate(int maxhp, int maxmp)
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

            if (localchairrate != -1)
            {
                return;
            }


            var p = getChairTaskIntervalRate(ActualMaxHP, ActualMaxMP);

            localchairrate = p.Rate;
            localchairhp = p.Hp;
            localchairmp = p.Mp;
        }

        public async Task<bool> unregisterChairBuff()
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
                return await cancelEffect(mapChairSkill, false);
            }

            return false;
        }

        public async Task<bool> registerChairBuff()
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
                await mapChairSkill.applyTo(this);
                return true;
            }

            return false;
        }

        public async Task ApplayChairBuff()
        {
            updateChairHealStats();
            int healHP = localchairhp;
            int healMP = localchairmp;

            if (HP < ActualMaxHP || MP < ActualMaxMP)
            {
                await UpdateStatsChunk(async () =>
                {
                    await ChangeHP(healHP);
                    ChangeMP(healMP);
                });

                var recHP = (sbyte)(healHP / YamlConfig.config.server.CHAIR_EXTRA_HEAL_MULTIPLIER);

                await SendPacket(PacketCreator.showOwnRecovery(recHP));
                await BroadcastMap(PacketCreator.showRecovery(Id, recHP), Id);
            }


        }
    }
}
