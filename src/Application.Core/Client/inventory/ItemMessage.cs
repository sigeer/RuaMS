using ProtoModel;

namespace client.inventory
{
    /// <summary>
    /// 礼物列表的一行：物品是中心服存的轻量条目，不需要为了显示生成真物品。
    /// </summary>
    public record ItemMessagePair(ItemProto Item, string? Message);
}
