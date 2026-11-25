namespace Application.Core.Game.Trades;

/// <summary>
/// 现金商城的会话对象：<see cref="Client.IChannelClient.TradeServer"/> 取这个值即表示角色正在商城界面里。
/// <para>
/// 每次进商城新建一份、离开即丢弃，所以这里只放「这次商城会话」才有效的数据（未读礼物条数）。
/// 余额、现金仓、格数全程由中心服持有，需要时用
/// <see cref="ServerTransports.IChannelServerTransport.GetCashShopState"/> 现查。
/// 愿望单属于角色数据，挂在 <see cref="Players.Player.WishList"/> 上，不在这里。
/// </para>
/// </summary>
public class CashShopServer : ITradeServer
{
    /// <summary>
    /// 点券
    /// </summary>
    public const int NX_CREDIT = CashType.NX_CREDIT;
    /// <summary>
    /// 抵用券
    /// </summary>
    public const int MAPLE_POINT = CashType.MAPLE_POINT;
    /// <summary>
    /// 信用卡
    /// </summary>
    public const int NX_PREPAID = CashType.NX_PREPAID;

    /// <summary>
    /// 从中心服回传的余额里取某一种点数。余额不在频道缓存，这是唯一的取值入口。
    /// </summary>
    public static int GetCash(ProtoModel.PlayerCashProto? cash, int cashType)
    {
        return cashType switch
        {
            NX_CREDIT => cash?.NxCredit ?? 0,
            MAPLE_POINT => cash?.MaplePoint ?? 0,
            NX_PREPAID => cash?.NxPrepaid ?? 0,
            _ => 0,
        };
    }

    /// <summary>
    /// 未读礼物的条数（进商城时收到的礼物数，回信会消耗一个）
    /// </summary>
    public int Notes { get; set; }
}
