namespace Application.Core.Game.Gameplay
{
    public record AttackTarget(short delay, List<AttackDamage> damageLines);

    public record AttackDamage(int Damage, bool IsCrit)
    {
        /// <summary>
        /// 客户端解析时通过bit31解析判定是否暴击
        /// </summary>
        /// <returns></returns>
        public int EncodeCritDamage()
        {
            return IsCrit ? unchecked(Damage + int.MinValue) : Damage;
        }
    }
}
