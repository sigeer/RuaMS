namespace Application.Core.Game.Maps.MiniRoom
{
    public enum PlayerShopCloseReason
    {
        /// <summary>
        /// 正常手动关闭
        /// </summary>
        Normal,
        /// <summary>
        /// 超时
        /// </summary>
        Expiration,
        /// <summary>
        /// 管理员处理
        /// </summary>
        Admin
    }
}
