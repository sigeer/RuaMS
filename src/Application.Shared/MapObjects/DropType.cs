namespace Application.Shared.MapObjects
{
    public enum DropType: byte
    {
        /// <summary>
        /// 限个人
        /// </summary>
        OnlyOwner,
        /// <summary>
        /// 全队
        /// </summary>
        OwnerWithTeam,
        /// <summary>
        /// 所有人
        /// </summary>
        FreeForAll,
        /// <summary>
        /// 不明
        /// </summary>
        FreeForAll_Explosive
    }
}
