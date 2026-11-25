using Application.Core.Channel.ServerData;

namespace Application.Core.Channel.Net.Handlers;

/// <summary>
/// 拍卖行窗口的「查询」按钮（C→S <c>0xFC</c>，裸包）
/// <para>
/// 客户端 <c>CITC</c> 主窗口在 <c>sub_5B8453</c> 里创建三个按钮：1000 = 充值、
/// 1001 = 查询（<c>SP_4737_UI_ITCIMG_BTCHECK</c>）、1002 = 离开，
/// 由 <c>sub_5B8688</c> 分发成裸包 <c>0xFB</c> / <c>0xFC</c> / <c>CHANGE_MAP</c>。
/// </para>
/// <para>
/// 0xFC 只要求回一条拍卖行余额（<c>MTS_OPERATION2</c>，<c>sub_5A428C</c>），列表不动。
/// 该处理函数在余额非负时会把「请求在途」标志 <c>+0x18</c> 清 0，所以单独发余额就够，
/// 不需要再刷一页。
/// </para>
/// </summary>
public class MtsCheckHandler : ChannelHandlerBase
{
    readonly MTSManager _mtsManager;

    public MtsCheckHandler(MTSManager mtsManager)
    {
        _mtsManager = mtsManager;
    }

    public override Task HandlePacket(InPacket p, IChannelClient c)
    {
        return _mtsManager.ShowCash(c);
    }
}
