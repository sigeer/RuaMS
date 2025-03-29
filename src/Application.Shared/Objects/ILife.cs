namespace Application.Shared.Objects
{
    public interface ILife
    {
        int MaxHP { get; }
        int HP { get; }
        int MaxMP { get; }
        int MP { get; }

        /// <summary>
        /// 如果是百分比加成，则基于基础
        /// </summary>
        int EquipMaxHP { get; }
        /// <summary>
        /// 如果是百分比加成，则基于基础+装备
        /// </summary>
        int BuffMaxHP { get; }
        int EquipMaxMP { get; }
        int BuffMaxMP { get; }

        int ActualMaxHP { get; }
        int ActualMaxMP { get; }

        void ChangeMaxHP(int value);
        void ChangeMaxMP(int value);
        /// <summary>
        /// 重算真实上限
        /// </summary>
        void RecalculateMaxHP();
        void RecalculateMaxMP();

        /// <summary>
        /// 修改当前血量，HP in [0, ActualMaxHP]
        /// </summary>
        /// <param name="deltaValue"></param>
        /// <param name="useCheck">是否校验当前血量</param>
        /// <returns>校验当前血量时，如果当前血量不足返回false</returns>
        bool ChangeHP(int deltaValue, bool useCheck = true);
        bool ChangeMP(int deltaValue, bool useCheck = true);
        void SetHP(int value);
        void SetMP(int value);
        void KilledBy(ILife killer);
    }
}
