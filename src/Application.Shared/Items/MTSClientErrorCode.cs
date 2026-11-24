namespace Application.Shared.Items;

/// <summary>
/// 拍卖行（MTS / ITC）的客户端错误码
/// <para>
/// 客户端 <c>CITC</c> 的 op 34 / 36 / 38 都是一个 byte 错误码，由 <c>sub_5A4752</c>（基址 0x400000）
/// 映射成提示框文案。op 34 / 36 在错误码为 73 时还会把玩家踢回原地图（<c>CITC::SendTransferFieldPacket</c>），
/// op 38 只弹提示。
/// </para>
/// <para>详见 docs/ida/MTS_拍卖行_封包分析.md</para>
/// </summary>
public enum MTSClientErrorCode : byte
{
    /// <summary>未知错误（客户端兜底文案：Due to an unknown error the request for MTS has failed）</summary>
    Unknown = 0,

    /// <summary>'B'：You do not have enough NX</summary>
    NotEnoughNX = 66,

    /// <summary>'C'：You do not have enough mesos</summary>
    NotEnoughMeso = 67,

    /// <summary>'D'：An error has occurred while using NX</summary>
    NxError = 68,

    /// <summary>'G'：Your inventory is full</summary>
    InventoryFull = 71,

    /// <summary>73：Failed to load the list（op 34 / 36 收到该码会把玩家送出拍卖行）</summary>
    FailedToLoadList = 73,

    /// <summary>'N'：Unable to pull out of the sale - you have at least 1 bid on the item</summary>
    CannotWithdrawHasBid = 78,

    /// <summary>'O'：This item may not be sold</summary>
    ItemMayNotBeSold = 79,

    /// <summary>'Q'：The item has been sold - you cannot cancel the sale of a purchased item</summary>
    AlreadySoldCannotCancel = 81,

    /// <summary>'R'：Throwing stars are not tradable</summary>
    ThrowingStarsNotTradable = 82,

    /// <summary>'S'：You must be over level 10 to sell items on MapleStory Trading System</summary>
    LevelTooLow = 83,
}
