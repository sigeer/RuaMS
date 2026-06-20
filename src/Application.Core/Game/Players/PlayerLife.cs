using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Shared.Objects;
using client.inventory.manipulator;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player : ILife
    {
        public int MaxHP { get; private set; }
        public int HP { get; private set; }
        public int MaxMP { get; private set; }
        public int MP { get; private set; }

        /// <summary>
        /// 如果是百分比加成，则基于基础
        /// </summary>
        public int EquipMaxHP { get; private set; }
        /// <summary>
        /// 如果是百分比加成，则基于基础+装备
        /// </summary>
        public int BuffMaxHP { get; private set; }
        public int EquipMaxMP { get; private set; }
        public int BuffMaxMP { get; private set; }
        public int ActualMaxHP { get; private set; }
        public int ActualMaxMP { get; private set; }
        public int HpAlert { get; set; } = DefaultConfigs.HPAlert;
        public int MpAlert { get; set; } = DefaultConfigs.MPAlert;

        public async Task ChangeMaxHP(int value)
        {
            var targetValue = MaxHP + value;
            await SetMaxHP(targetValue);
        }
        public void ChangeMaxMP(int value)
        {
            var targetValue = MaxMP + value;
            SetMaxMP(targetValue);
        }

        private async Task SetMaxHP(int value)
        {
            if (value < NumericConfig.MinHp)
                value = NumericConfig.MinHp;
            if (value > NumericConfig.MaxHP)
                value = NumericConfig.MaxHP;

            if (value == MaxHP)
                return;

            MaxHP = value;
            statUpdates[Stat.MAXHP] = MaxHP;
            await RecalculateMaxHP();
        }

        private void SetMaxMP(int value)
        {
            if (value < NumericConfig.MinMP)
                value = NumericConfig.MinMP;
            if (value > NumericConfig.MaxMP)
                value = NumericConfig.MaxMP;

            if (value == MaxMP)
                return;

            MaxMP = value;
            statUpdates[Stat.MAXMP] = MaxMP;
            RecalculateMaxMP();
        }
        public async Task<bool> ChangeHP(int deltaValue, bool useCheck = true)
        {
            if (deltaValue == 0)
                return true;

            var targetValue = HP + deltaValue;
            if (useCheck && targetValue <= 0)
                return false;

            await SetHP(targetValue);
            return true;
        }
        public bool ChangeMP(int deltaValue, bool useCheck = true)
        {
            var targetValue = MP + deltaValue;
            if (useCheck && targetValue < 0)
                return false;

            SetMP(targetValue);
            return true;
        }

        public async Task SetHP(int value)
        {
            if (value < 0)
                value = 0;
            if (value > ActualMaxHP)
                value = ActualMaxHP;

            if (value == HP)
                return;

            HP = value;
            statUpdates[Stat.HP] = HP;

            if (MapModel != null)
            {
                await updatePartyMemberHP();

                if (HP <= 0)
                {
                    await OnDead();
                }
                else
                {
                    // 每次血量变化，都会重置这个技能效果的生效时间？
                    // checkBerserk(isHidden());
                }
            }
        }

        public async Task UpdateHP(int value)
        {
            await UpdateStatsChunk(async () =>
              {
                  await SetHP(value);
              });
        }

        public void SetMP(int value)
        {
            if (value < 0)
                value = 0;
            if (value > ActualMaxMP)
                value = ActualMaxMP;

            if (value == MP)
                return;

            MP = value;
            statUpdates[Stat.MP] = MP;
        }

        private void RefreshByBuff()
        {
            var oldBuffMaxHP = BuffMaxHP;
            var hbhp = getBuffedValue(BuffStat.HYPERBODYHP);
            if (hbhp != null)
            {
                BuffMaxHP = (int)((hbhp.Value / 100.0) * (MaxHP + EquipMaxHP));
            }
            else
            {
                BuffMaxHP = 0;
            }

            var oldBuffMaxMP = BuffMaxMP;
            var buffMP = getBuffedValue(BuffStat.HYPERBODYMP);
            if (buffMP != null)
            {
                BuffMaxMP = (int)((buffMP.Value / 100.0) * (MaxMP + EquipMaxMP));
            }
            else
            {
                BuffMaxMP = 0;
            }
        }
        private void RefreshByEquip(int hp, int mp)
        {
            if (EquipMaxHP != hp)
            {
                EquipMaxHP = hp;
            }

            if (EquipMaxMP != mp)
            {
                EquipMaxMP = mp;
            }
        }

        private async Task RecalculateMaxHP()
        {
            var newMaxHp = (int)(MaxHP + BuffMaxHP + EquipMaxHP);
            if (newMaxHp != ActualMaxHP)
            {
                ActualMaxHP = newMaxHp;

                await SetHP(HP);
            }
        }

        private void RecalculateMaxMP()
        {
            var newMaxMp = (int)(MaxMP + EquipMaxMP + BuffMaxMP);
            if (newMaxMp != ActualMaxMP)
            {
                ActualMaxMP = newMaxMp;

                SetMP(MP);
            }
        }

        public async Task KilledBy(ILife killer)
        {
            await UpdateStatsChunk(async () =>
            {
                await SetHP(0);
            });
        }

        // 需要在UpdateStatsChunk内调用
        public async Task<bool> DamageBy(ICombatantObject? attacker, int damageValue, short delay, bool stayAlive = false)
        {
            if (!isAlive())
            {
                return false;
            }

            if (stayAlive)
                damageValue = Math.Min(HP, damageValue) - 1;

            await ChangeHP(-damageValue, false);
            return true;
        }

        async Task OnDead()
        {
            await cancelAllBuffs(false);
            await dispelDebuffs();
            lastDeathtime = Client.CurrentServer.Node.getCurrentTime();

            var eim = getEventInstance();
            if (eim != null)
            {
                eim.playerKilled(this);
            }
            usedSafetyCharm = false;

            if (JobModel != Job.BEGINNER
                && !MapId.isDojo(getMapId())
                && eim is not MonsterCarnivalEventInstanceManager
                && !FieldLimit.NO_EXP_DECREASE.check(MapModel.getFieldLimit()))
            {

                for (var i = 0; i < ItemId.SafetyCharms.Length; i++)
                {
                    var invType = ItemConstants.getInventoryType(ItemId.SafetyCharms[i]);
                    var inv = Bag[invType];
                    var itemCount = inv.countById(ItemId.SafetyCharms[i]);
                    if (itemCount > 0)
                    {
                        await InventoryManipulator.removeById(Client, invType, ItemId.SafetyCharms[i], 1, true, false);
                        usedSafetyCharm = true;
                        await SendPacket(EffectPacket.ExpDidNotDrop(ItemId.SafetyCharms[i]));
                        break;
                    }
                }

                if (!usedSafetyCharm)
                {
                    // thanks Conrad for noticing missing FieldLimit check
                    int XPdummy = ExpTable.getExpNeededForLevel(getLevel());

                    if (MapModel.SourceTemplate.Town)
                    {    // thanks MindLove, SIayerMonkey, HaItsNotOver for noting players only lose 1% on town maps
                        XPdummy /= 100;
                    }
                    else
                    {
                        if (getLuk() < 50)
                        {    // thanks Taiketo, Quit, Fishanelli for noting player EXP loss are fixed, 50-LUK threshold
                            XPdummy /= 10;
                        }
                        else
                        {
                            XPdummy /= 20;
                        }
                    }

                    int curExp = getExp();
                    if (curExp > XPdummy)
                    {
                        await loseExp(XPdummy, false, false);
                    }
                    else
                    {
                        await loseExp(curExp, false, false);
                    }
                }
            }

            await cancelEffectFromBuffStat(BuffStat.MORPH);

            await cancelEffectFromBuffStat(BuffStat.MONSTER_RIDING);

            await unsitChairInternal();
            await SendPacket(PacketCreator.enableActions());
        }

    }
}
