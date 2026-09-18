namespace Application.Shared.Battle
{
    public enum AttackIndex: sbyte
    {
        /// <summary>
        /// attack1/2/...
        /// </summary>
        Maigic = 0,
        /// <summary>
        /// 碰撞
        /// </summary>
        Physical = -1,

        /// <summary>
        /// 伤害反击
        /// </summary>
        Counter = -2,
        /// <summary>
        /// 地图障碍、陷阱等
        /// </summary>
        Obstacle = -3,
    }
}
