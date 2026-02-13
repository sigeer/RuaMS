namespace Application.Shared.MapObjects
{
    public enum DropType: byte
    {
        /// <summary>
        /// 个人可拾取
        /// </summary>
        OnlyOwner,
        /// <summary>
        /// 全队可拾取
        /// </summary>
        OnwerWithTeam,
        /// <summary>
        /// 所有人可拾取
        /// </summary>
        FreeForAll,
        /// <summary>
        /// 金钱炸弹掉落，所有人可拾取
        /// </summary>
        FreeForAll_Explosive
    }
}
