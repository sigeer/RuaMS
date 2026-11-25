namespace Application.Core.Game.Trades;

/// <summary>
/// 客户端当前所处的「离世界」交易界面（商城 / 拍卖行）。
/// <para>
/// <c>IChannelClient.TradeServer</c> 非空即表示角色已经脱离世界进入对应界面；
/// 具体是哪一个由实现类型区分（<see cref="CashShopServer"/> / <see cref="MtsServer"/>）。
/// </para>
/// </summary>
public interface ITradeServer
{
}
