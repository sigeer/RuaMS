namespace Application.Shared.Battle
{
    public enum DamageFromTypes
    {
        Maigic = 0,
        Physical = -1,

        /// <summary>
        /// 未确认
        /// </summary>
        Counter = -2,
        /// <summary>
        /// 地图障碍、陷进等
        /// </summary>
        Obstacle = -3,
        /// <summary>
        /// 未确认
        /// </summary>
        Stat = -4
    }
}
