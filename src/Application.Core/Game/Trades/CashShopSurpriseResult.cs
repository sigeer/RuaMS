namespace Application.Core.Game.Trades;

/// <summary>
/// 现金扭蛋的结果：被消耗的那件、消耗后剩下的数量、抽到的奖品
/// （都是中心服的轻量条目，不是真物品）
/// </summary>
public record CashShopSurpriseResult(ProtoModel.ItemProto Used, int RemainingQuantity, ProtoModel.ItemProto Reward);
