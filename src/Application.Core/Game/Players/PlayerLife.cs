using Application.Shared.Objects;
using client;

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

        public void ChangeMaxHP(int value)
        {
            var targetValue = MaxHP + value;
            SetMaxHP(targetValue);
        }
        public void ChangeMaxMP(int value)
        {
            var targetValue = MaxMP + value;
            SetMaxMP(targetValue);
        }

        private void SetMaxHP(int value)
        {
            if (value < NumericConfig.MinHp)
                value = NumericConfig.MinHp;
            if (value > NumericConfig.MaxHP)
                value = NumericConfig.MaxHP;

            if (value == MaxHP)
                return;

            MaxHP = value;
            statUpdates[Stat.MAXHP] = MaxHP;
            RecalculateMaxHP();
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
        public bool ChangeHP(int deltaValue, bool useCheck = true)
        {
            var targetValue = HP + deltaValue;
            if (useCheck && targetValue <= 0)
                return false;

            SetHP(targetValue);
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

        public void SetHP(int value)
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
                var died = HP <= 0;
                MapModel.registerCharacterStatUpdate(() =>
                {
                    updatePartyMemberHP();    // thanks BHB (BHB88) for detecting a deadlock case within player stats.

                    if (died)
                    {
                        playerDead();
                    }
                    else
                    {
                        checkBerserk(isHidden());
                    }
                });
            }
        }

        public void UpdateHP(int value)
        {
            UpdateStatsChunk(() =>
            {
                SetHP(value);
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

        private void RecalculateMaxHP()
        {
            var newMaxHp = (int)(MaxHP + BuffMaxHP + EquipMaxHP);
            if (newMaxHp != ActualMaxHP)
            {
                ActualMaxHP = newMaxHp;

                SetHP(HP);
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

        public void KilledBy(ILife killer)
        {
            UpdateStatsChunk(() =>
            {
                SetHP(0);
            });
        }
    }
}
