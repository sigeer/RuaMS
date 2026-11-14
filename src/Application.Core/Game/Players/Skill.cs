using Application.Core.Game.Gameplay;
using Application.Core.Game.Players.PlayerProps;
using Application.Core.Game.Players.Tickables;
using Application.Core.Game.Skills;
using Application.Core.Server;
using Application.Shared.Battle.Skills;
using Application.Shared.KeyMaps;
using net.server;
using System.Collections;
using System.Runtime.ConstrainedExecution;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public PlayerSkill Skills { get; set; }
        public Dictionary<int, int> TempSkillCache { get; set; } = new();
        /// <summary>
        /// skillId - Cooldown
        /// </summary>
        private Dictionary<int, PlayerCoolDownValueHolder> coolDowns = new();
        public Dictionary<Skill, SkillEntry> getSkills()
        {
            return Skills.GetDataSource();
        }

        /// <summary>
        /// 玩家技能学习等级
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        public int GetPlayerSkillLevel(int skill)
        {
            return getSkillLevel(SkillFactory.getSkill(skill), false);
        }

        public int getSkillLevel(int skill)
        {
            return getSkillLevel(SkillFactory.getSkill(skill));
        }

        /// <summary>
        /// 玩家技能等级
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="final">true: 取最终效果（+装备技能等级）</param>
        /// <returns></returns>
        public sbyte getSkillLevel(Skill? skill, bool final = true)
        {
            if (skill == null)
            {
                return 0;
            }
            return (sbyte)Math.Min(
                (Skills.GetSkill(skill)?.skillevel ?? 0) + (final ? TempSkillCache.GetValueOrDefault(skill.getId()) : 0),
                skill.getMaxLevel());
        }

        /// <summary>
        /// 根据玩家技能等级获取玩家技能效果
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="checkJob">验证职业<para>正常途径不会出现跨职业调用技能效果</para></param>
        /// <returns></returns>
        public StatEffect? GetPlayerSkillEffect(int skillId, bool checkJob = true)
        {
            var skillObj = SkillFactory.GetSkillTrust(skillId);

            if (!checkJob || JobModel.CheckSkill(skillId))
            {
                var skillLevel = getSkillLevel(skillObj);
                if (skillLevel == 0)
                    return null;
                return skillObj.getEffect(skillLevel);
            }

            return null;
        }

        public async Task changeSkillLevel(Skill skill, sbyte newLevel, int newMasterlevel, long expiration)
        {
            if (newLevel > -1)
            {
                Skills.AddOrUpdate(skill, new SkillEntry(newLevel, newMasterlevel, expiration));
                if (!GameConstants.isHiddenSkills(skill.getId()))
                {
                    await SendPacket(PacketCreator.updateSkill(skill.getId(), newLevel, newMasterlevel, expiration));
                }
            }
            else
            {
                Skills.Remove(skill);
                await SendPacket(PacketCreator.updateSkill(skill.getId(), newLevel, newMasterlevel, -1)); //Shouldn't use expiration anymore :)
            }
        }

        public int getMasterLevel(int skill)
        {
            var skillData = SkillFactory.getSkill(skill);
            if (skillData == null)
            {
                return 0;
            }
            return getMasterLevel(skillData);
        }

        public int getMasterLevel(Skill? skill)
        {
            if (skill == null)
                return 0;

            var characterSkill = Skills.GetSkill(skill);
            if (characterSkill == null)
            {
                return 0;
            }
            return characterSkill.masterlevel;
        }

        public long getSkillExpiration(int skill)
        {
            var skillData = SkillFactory.getSkill(skill);
            if (skillData == null)
                return -1;
            return getSkillExpiration(skillData);
        }

        public long getSkillExpiration(Skill? skill)
        {
            return skill == null ? -1 : (Skills.GetSkill(skill)?.expiration ?? -1);
        }

        #region skill macro
        public SkillMacro?[] SkillMacros { get; set; }
        public async Task sendMacros()
        {
            // Always send the macro packet to fix a Client side bug when switching characters.
            await SendPacket(PacketCreator.getMacros(SkillMacros));
        }

        public SkillMacro?[] getMacros()
        {
            return SkillMacros;
        }

        public void updateMacros(int position, SkillMacro? updateMacro)
        {
            SkillMacros[position] = updateMacro;
        }
        #endregion

        #region skill cooldown
        public List<PlayerCoolDownValueHolder> getAllCooldowns()
        {
            return coolDowns.Values.ToList();
        }
        public bool skillIsCooling(int skillId)
        {
            return coolDowns.ContainsKey(skillId);
        }

        public async Task ClearExpiredSkillCooldown(long now)
        {
            var es = coolDowns.ToHashSet();

            foreach (var bel in es)
            {
                if (bel.Key == Corsair.BATTLE_SHIP_HP)
                {
                    continue;
                }

                var mcdvh = bel.Value;
                if (now >= mcdvh.startTime + mcdvh.length)
                {
                    removeCooldown(mcdvh.skillId);
                    await SendPacket(PacketCreator.skillCooldown(mcdvh.skillId, 0));
                }
            }
        }

        public async Task removeAllCooldownsExcept(int id, bool packet)
        {
            var list = coolDowns.Values.ToList();
            foreach (var mcvh in list)
            {
                if (mcvh.skillId != id)
                {
                    coolDowns.Remove(mcvh.skillId);
                    if (packet)
                    {
                        await SendPacket(PacketCreator.skillCooldown(mcvh.skillId, 0));
                    }
                }
            }
        }

        public void removeCooldown(int skillId)
        {
            this.coolDowns.Remove(skillId);
        }
        public void addCooldown(int skillId, long startTime, long length)
        {
            this.coolDowns.AddOrUpdate(skillId, new(skillId, startTime, length));
        }
        public void giveCoolDowns(int skillid, long starttime, long length)
        {
            if (skillid == Corsair.BATTLE_SHIP_HP)
            {
                this.battleshipHp = (int)length;
                addCooldown(skillid, 0, length);
            }
            else
            {
                long timeNow = Client.CurrentServer.Node.getCurrentTime();
                int time = (int)((length + starttime) - timeNow);
                addCooldown(skillid, timeNow, time);
            }
        }
        #endregion

        public int GetMakerSkillLevel()
        {
            return getSkillLevel((getJob().getId() / 1000) * 10000000 + 1007);
        }

        public async Task<bool> LearnSkill(int skillId, int skillLevel = -1)
        {
            var skill = SkillFactory.getSkill(skillId);
            if (skill == null)
            {
                return false;
            }
            var level = skillLevel == -1 ? skill.getMaxLevel() : skillLevel;
            await changeSkillLevel(skill, (sbyte)level, level, -1);
            changeKeybinding((int)KeyCode.Equal, new KeyBinding(KeyBindingType.Skill, skillId));
            await sendKeymap();
            return true;
        }

        public bool CheckBuff(EffectBuff buff)
        {
            if (!buff.Effect.isSkill())
            {
                return true;
            }
            return JobModel.CheckSkill(buff.Effect.getSourceId());
        }

        /// <summary>
        /// 技能释放检测
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="skillLevel"></param>
        /// <param name="attackInfo">攻击性技能</param>
        /// <returns></returns>
        public async Task<bool> SkillUsed(int skillId, int skillLevel, AttackInfo? attackInfo = null)
        {
            StatEffect? skillEffect = null;

            if (skillId > 0)
            {
                skillEffect = GetPlayerSkillEffect(skillId, !isGM());
                if (skillEffect == null)
                    return false;
            }


            if (attackInfo != null)
            {
                // 无法攻击的变身状态
                if (IsMorphWithoutAttack())
                {
                    await Client.Disconnect(false, false);
                    return false;
                }

                if (skillId != attackInfo.skill && skillLevel != attackInfo.skilllevel)
                {
                    return false;
                }
            }

            if (skillEffect != null)
            {
                if (skillLevel > skillEffect.SkillLevel)
                {
                    await Client.Disconnect(false, false);
                    return false;
                }

                // 技能是否处于CD
                if (skillEffect.getCooldown() > 0)
                {
                    if (skillIsCooling(skillEffect.getBuffSourceId()))
                    {
                        return false;
                    }
                    else if (skillEffect.getBuffSourceId() != Corsair.BATTLE_SHIP)
                    {
                        // 武装 是在被破坏后进入CD
                        int cooldownTime = skillEffect.getCooldown();
                        if (StatEffect.isHerosWill(skillEffect.getBuffSourceId()) && YamlConfig.config.server.USE_FAST_REUSE_HERO_WILL)
                        {
                            cooldownTime /= 60;
                        }

                        await SendPacket(PacketCreator.skillCooldown(skillEffect.getBuffSourceId(), cooldownTime));
                        addCooldown(skillEffect.getBuffSourceId(), Client.CurrentServer.Node.getCurrentTime(), 1000 * cooldownTime);
                    }
                }
            }

            if (attackInfo != null)
            {
                if (HasBuff(BuffStat.DARKSIGHT))
                {
                    await CancelBuff(BuffStat.DARKSIGHT);
                }
            }

            if (MapId.isDojo(MapModel.Id))
            {
                var tSkill = skillId % 10000000;
                if (tSkill == 1009 || tSkill == 1010 || tSkill == 1011)
                {
                    if (getDojoEnergy() < 10000)
                    {
                        return false;
                    }

                    setDojoEnergy(0);
                    await Pink("As you used the secret skill, your energy bar has been reset.");
                }
                else if (attackInfo?.numAttacked > 0)
                {
                    setDojoEnergy(getDojoEnergy() + +YamlConfig.config.server.DOJO_ENERGY_ATK);
                }
                await SendPacket(PacketCreator.getEnergy(SessionValues.Energy, getDojoEnergy()));
            }

            return true;
        }

    }
}
