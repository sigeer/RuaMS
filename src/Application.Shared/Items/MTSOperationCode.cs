namespace Application.Shared.Items;

/// <summary>
/// 拍卖行 op 30（<c>MTS_OPERATION</c> 的结果码）枚举。
/// <para>
/// 和 <see cref="MTSClientErrorCode"/> <b>不是同一套</b>：后者是 op 34 / 36 / 38 的
/// 「错误码」，任意 byte 都会走 <c>sub_5A4752</c> 的文案表；而 op 30 的客户端处理
/// <c>sub_5A4581</c> 只认下表中的码，其余码直接静默（仅清掉「请求在途」标志）。
/// </para>
/// <list type="bullet">
/// <item><b>72</b> 之后还必须再写一个 <c>short</c>（数量上限），用
/// <c>MTSPacketCreator.MTSLimitUnits</c>；</item>
/// <item><b>63 / 68</b> 会把玩家强制送出拍卖行（<c>CITC::SendTransferFieldPacket</c>）；
/// </item>
/// <item>其余列出的码只弹提示。</item>
/// </list>
/// <para>详见 docs/ida/MTS_拍卖行_封包分析.md §13.2。</para>
/// </summary>
public enum MTSOperationCode : byte
{
    /// <summary>
    /// 0：客户端没有对应文案，只会清掉「请求在途」标志（静默）。
    /// <para>用于「中心服拒绝了但客户端没有专门提示」的场景。</para>
    /// </summary>
    Unknown = 0,
    /// <summary>63 (0x3F)：弹兜底提示并把玩家送出拍卖行</summary>
    KickUnknown = 63,
    /// <summary>66 'B'：You do not have enough NX</summary>
    NotEnoughNX = 66,
    /// <summary>67 'C'：You do not have enough mesos</summary>
    NotEnoughMeso = 67,
    /// <summary>68 'D'：An error has occurred while using NX —— 弹提示并送出拍卖行</summary>
    NxError = 68,
    /// <summary>71 'G'：Your inventory is full</summary>
    InventoryFull = 71,
    /// <summary>72 (0x48)：数量上限。**必须**再跟一个 short，见 MTSLimitUnits</summary>
    LimitUnits = 72,
    /// <summary>79 'O'：This item may not be sold</summary>
    ItemMayNotBeSold = 79,
    /// <summary>82 'R'：Throwing stars are not tradable</summary>
    ThrowingStarsNotTradable = 82,
    /// <summary>83 'S'：You must be over level 10 to sell items on MTS</summary>
    LevelTooLow = 83,
}