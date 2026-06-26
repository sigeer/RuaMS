using Application.Core.Channel.AntiMacro;
using Application.Core.Channel.Net.Packets;
using Application.Core.Net;
using client.inventory.manipulator;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/// <summary>
/// 处理 CWvsContext::SendAntiMacroItemUseRequest (RecvOpcode 103)。
/// </summary>
public class AntiMacroItemUseRequestHandler : ChannelHandlerBase
{
    private readonly AntiMacroService _antiMacro;

    public AntiMacroItemUseRequestHandler(AntiMacroService antiMacro)
    {
        _antiMacro = antiMacro;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;

        if (!chr.isAlive())
        {
            await c.SendPacket(PacketCreator.enableActions());
            return;
        }

        // ---- 读取客户端数据包 ----
        var targetName = p.readString();
        var slot = p.readShort();
        var itemId = p.readInt();

        // ---- 验证物品 ----
        if (itemId != ItemId.AntiMacroItem)
        {
            await c.SendPacket(PacketCreator.enableActions());
            return;
        }

        var inv = chr.getInventory(InventoryType.USE);
        var item = inv.getItem(slot);
        if (item == null || item.getItemId() != itemId || item.getQuantity() <= 0)
        {
            await c.SendPacket(PacketCreator.enableActions());
            return;
        }

        // ---- 验证目标 ----
        var target = chr.getMap().getCharacterByName(targetName);
        if (target == null || !target.IsOnlined || target == chr)
        {
            await c.SendPacket(AntiMacroPackets.PlayerNotFound());
            return;
        }

        // ---- 发包 + 超时检测 ----
        await _antiMacro.SendAntiMacroAsync(chr, target, AntiMacroType.Item, onSuccess:async () =>
        {
            await InventoryManipulator.removeFromSlot(c, InventoryType.USE, slot, 1, false);
        });

        await c.SendPacket(PacketCreator.enableActions());
    }
}
