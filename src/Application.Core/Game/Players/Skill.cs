using Application.Core.Game.Players.PlayerProps;
using Application.Core.Game.Skills;
using net.server;
using server;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public PlayerSkill Skills { get; set; }
        /// <summary>
        /// skillId - Cooldown
        /// </summary>
        private Dictionary<int, CooldownValueHolder> coolDowns = new();
        public Dictionary<Skill, SkillEntry> getSkills()
        {
            return Skills.GetDataSource();
        }

        public int getSkillLevel(int skill)
        {
            return getSkillLevel(SkillFactory.getSkill(skill));
        }

        public sbyte getSkillLevel(Skill? skill)
        {
            return skill == null ? (sbyte)0 : (Skills.GetSkill(skill)?.skillevel ?? 0);
        }

        public StatEffect GetPlayerSkillEffect(int skillId)
        {
            var skillObj = SkillFactory.GetSkillTrust(skillId);
            return GetPlayerSkillEffect(skillObj);
        }

        public StatEffect GetPlayerSkillEffect(Skill skill)
        {
            var skillLevel = getSkillLevel(skill);
            if (skillLevel == 0)
                throw new BusinessResException($"Id = {Id} Name = {Name}, SkillId = {skill.getId()}, PlayerSkillLevel = 0");
            return skill.getEffect(skillLevel);
        }

        public void changeSkillLevel(Skill skill, sbyte newLevel, int newMasterlevel, long expiration)
        {
            if (newLevel > -1)
            {
                Skills.AddOrUpdate(skill, new SkillEntry(newLevel, newMasterlevel, expiration));
                if (!GameConstants.isHiddenSkills(skill.getId()))
                {
                    sendPacket(PacketCreator.updateSkill(skill.getId(), newLevel, newMasterlevel, expiration));
                }
            }
            else
            {
                Skills.Remove(skill);
                sendPacket(PacketCreator.updateSkill(skill.getId(), newLevel, newMasterlevel, -1)); //Shouldn't use expiration anymore :)
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
        public void sendMacros()
        {
            // Always send the macro packet to fix a Client side bug when switching characters.
            sendPacket(PacketCreator.getMacros(SkillMacros));
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
        private ScheduledFuture? _skillCooldownTask = null;
        public List<PlayerCoolDownValueHolder> getAllCooldowns()
        {
            List<PlayerCoolDownValueHolder> ret = new();

            Monitor.Enter(effLock);
            chLock.EnterReadLock();
            try
            {
                foreach (CooldownValueHolder mcdvh in coolDowns.Values)
                {
                    ret.Add(new PlayerCoolDownValueHolder(mcdvh.skillId, mcdvh.startTime, mcdvh.length));
                }
            }
            finally
            {
                chLock.ExitReadLock();
                Monitor.Exit(effLock);
            }

            return ret;
        }
        public bool skillIsCooling(int skillId)
        {
            Monitor.Enter(effLock);
            chLock.EnterReadLock();
            try
            {
                return coolDowns.ContainsKey(skillId);
            }
            finally
            {
                chLock.ExitReadLock();
                Monitor.Exit(effLock);
            }
        }

        public void cancelSkillCooldownTask()
        {
            if (_skillCooldownTask != null)
            {
                _skillCooldownTask.cancel(false);
                _skillCooldownTask = null;
            }
        }

        public void skillCooldownTask()
        {
            if (_skillCooldownTask == null)
            {
                _skillCooldownTask = Client.CurrentServerContainer.TimerManager.register(() =>
                {
                    HashSet<KeyValuePair<int, CooldownValueHolder>> es;

                    Monitor.Enter(effLock);
                    chLock.EnterReadLock();
                    try
                    {
                        es = new(coolDowns);
                    }
                    finally
                    {
                        chLock.ExitReadLock();
                        Monitor.Exit(effLock);
                    }

                    long curTime = Client.CurrentServerContainer.getCurrentTime();
                    foreach (var bel in es)
                    {
                        CooldownValueHolder mcdvh = bel.Value;
                        if (curTime >= mcdvh.startTime + mcdvh.length)
                        {
                            removeCooldown(mcdvh.skillId);
                            sendPacket(PacketCreator.skillCooldown(mcdvh.skillId, 0));
                        }
                    }
                }, 1500);
            }
        }

        public void removeAllCooldownsExcept(int id, bool packet)
        {
            Monitor.Enter(effLock);
            chLock.EnterReadLock();
            try
            {
                List<CooldownValueHolder> list = new(coolDowns.Values);
                foreach (CooldownValueHolder mcvh in list)
                {
                    if (mcvh.skillId != id)
                    {
                        coolDowns.Remove(mcvh.skillId);
                        if (packet)
                        {
                            sendPacket(PacketCreator.skillCooldown(mcvh.skillId, 0));
                        }
                    }
                }
            }
            finally
            {
                chLock.ExitReadLock();
                Monitor.Exit(effLock);
            }
        }

        public void removeCooldown(int skillId)
        {
            Monitor.Enter(effLock);
            chLock.EnterReadLock();
            try
            {
                this.coolDowns.Remove(skillId);
            }
            finally
            {
                chLock.ExitReadLock();
                Monitor.Exit(effLock);
            }
        }
        public void addCooldown(int skillId, long startTime, long length)
        {
            Monitor.Enter(effLock);
            chLock.EnterReadLock();
            try
            {
                this.coolDowns.AddOrUpdate(skillId, new CooldownValueHolder(skillId, startTime, length));
            }
            finally
            {
                chLock.ExitReadLock();
                Monitor.Exit(effLock);
            }
        }
        public void giveCoolDowns(int skillid, long starttime, long length)
        {
            if (skillid == 5221999)
            {
                this.battleshipHp = (int)length;
                addCooldown(skillid, 0, length);
            }
            else
            {
                long timeNow = Client.CurrentServerContainer.getCurrentTime();
                int time = (int)((length + starttime) - timeNow);
                addCooldown(skillid, timeNow, time);
            }
        }
        #endregion

        public int GetMakerSkillLevel()
        {
            return getSkillLevel((getJob().getId() / 1000) * 10000000 + 1007);
        }
    }
}
