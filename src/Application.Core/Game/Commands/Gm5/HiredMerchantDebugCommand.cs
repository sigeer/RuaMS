using Application.Core.Game.Trades;
using Application.Core.scripting.npc;
using System.Reflection;

namespace Application.Core.Game.Commands.Gm5;

/// <summary>
/// 开发辅助命令：调试 HiredMerchant 过期时间
///   !hiredmerchant_debug info <mapObjectId>            — 查看商店信息
///   !hiredmerchant_debug set <mapObjectId> [minutes]   — 将过期时间设为 N 分钟后（默认 1 分钟）
/// </summary>
public class HiredMerchantDebugCommand : ParamsCommandBase
{
    public HiredMerchantDebugCommand() : base(["[info|set]", "<mapObjectId>"], 5, "hiredmerchant_debug")
    {
        Description = "调试 HiredMerchant 过期时间。";
    }

    public override bool CheckArguments(string[] values)
    {
        if (values.Length < 2) return false;
        if (values[0] is not "info" and not "set") return false;
        if (!int.TryParse(values[1], out _)) return false;
        return true;
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        var subCmd = paramsValue[0];
        var oid = int.Parse(paramsValue[1]);

        var obj = player.getMap().getMapObject(oid);
        if (obj is not HiredMerchant merchant)
        {
            await player.Yellow($"未找到 mapObjectId={oid} 对应的雇佣商店。");
            return;
        }

        switch (subCmd)
        {
            case "info":
                await ShowInfo(c, merchant);
                break;

            case "set":
                int minutes = 1;
                if (paramsValue.Length >= 3 && !int.TryParse(paramsValue[2], out minutes))
                {
                    await player.Yellow("参数错误：分钟数应为整数。");
                    return;
                }

                var now = player.getClient().CurrentServer.Node.getCurrentTime();
                var newExpiry = now + (long)TimeSpan.FromMinutes(minutes).TotalMilliseconds;
                var oldRemainingSec = Math.Max(0, (merchant.ExpirationTime - now) / 1000);
                typeof(HiredMerchant).GetField("<ExpirationTime>k__BackingField",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(merchant, newExpiry);

                await TempConversation.CreateScope(c, async ctx =>
                {
                    await ctx.SayOK($"""
                        已修改商店过期时间。
                        所有者: {merchant.OwnerName}
                        修改前剩余: {oldRemainingSec} 秒
                        过期时间: {DateTimeOffset.FromUnixTimeMilliseconds(newExpiry).ToLocalTime():HH:mm:ss}
                        """);
                });
                break;
        }
    }

    private static async Task ShowInfo(IChannelClient c, HiredMerchant merchant)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var start = DateTimeOffset.FromUnixTimeMilliseconds((long)merchant.StartTime);
        var expiry = DateTimeOffset.FromUnixTimeMilliseconds((long)merchant.ExpirationTime);
        var remainingSec = Math.Max(0, (merchant.ExpirationTime - now) / 1000);

        await TempConversation.CreateScope(c, async ctx =>
        {
            await ctx.SayOK($"""
                商店信息
                ────────────────────────
                所有者: {merchant.OwnerName} (ID: {merchant.OwnerId})
                状态:   {merchant.Status}
                创建:   {start:yyyy-MM-dd HH:mm:ss}
                过期:   {expiry:yyyy-MM-dd HH:mm:ss}
                剩余:   {remainingSec} 秒
                商品:   {merchant.Commodity.Count} 个
                金币:   {merchant.Mesos} meso
                访客:   {merchant.GetVisitorCount()} 人
                ────────────────────────
                """);
        });
    }
}
