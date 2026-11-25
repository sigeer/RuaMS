namespace Application.Core.Game.Players;

public partial class Player
{
    /// <summary>
    /// 愿望单。写回中心服，登录时从角色数据带回来；角色信息封包（SET_FIELD）也带这一段。
    /// </summary>
    public List<int> WishList { get; private set; } = new();

    public void LoadWishList(IEnumerable<int> wishList)
    {
        WishList = wishList.ToList();
    }
}
