namespace Application.Core.Game.Maps
{
    /// <summary>
    /// 可战斗对象：Player, Mob
    /// </summary>
    public interface ICombatantObject: IMapObject
    {
        /// <summary>
        /// 被造成伤害
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="damageValue"></param>
        /// <param name="delay"></param>
        /// <param name="stayAlive">true: 不致命</param>
        /// <returns></returns>
        bool DamageBy(ICombatantObject attacker, int damageValue, short delay, bool stayAlive = false);
    }
}
