namespace Application.Core.Game.Gameplay;

/// <summary>
/// 攻击包伤害行的解码，兼容两种客户端。
/// <para>
/// <b>打过补丁的客户端</b>（BeiDou-ijl15 <c>Client::EnableCritFlagInAttackPacket</c>）
/// 会在发包前把 <c>target[21+i]</c>（暴击标记）OR 进 bit31，所以 <c>raw &lt; 0</c>
/// 表示"这一行带暴击标记"，低 31 位是真实伤害。
/// </para>
/// <para>
/// <b>原版客户端</b>的伤害恒为 <c>&gt;= 0</c>：<c>CalcDamage::PDamage</c> 在 0x790586
/// 用 <c>max(1.0, min(raw, dbl_AFE8A0))</c> 钳制后才写进 <c>target[6+i]</c>，不可能为负。
/// 因此非负数只可能来自原版客户端，此时暴击需要另行推断。
/// </para>
/// <para>
/// 二进制布局见 <c>docs/ida/CUserRemote_OnAttack_暴击伤害编码_分析与实现.md</c>。
/// </para>
/// </summary>
public readonly record struct DamageLine(bool HasCritFlag, bool IsCrit, int Damage)
{
    /// <summary>
    /// 解析一条伤害行。
    /// </summary>
    /// <param name="rawDamage">包里的原始 32 位值</param>
    public static DamageLine Decode(int rawDamage)
    {
        if (rawDamage < 0)
        {
            return new DamageLine(true, (rawDamage & int.MinValue) != 0, rawDamage & int.MaxValue);
        }
        return new DamageLine(false, false, rawDamage);
    }
}
