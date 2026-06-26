using Application.Core.Channel.AntiMacro;
using Application.Core.Net;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/// <summary>
/// 处理目标提交测谎答案 (Opcode 105)。
/// </summary>
public class AntiMacroResponseHandler : ChannelHandlerBase
{
    private readonly AntiMacroService _antiMacro;

    public AntiMacroResponseHandler(AntiMacroService antiMacro)
    {
        _antiMacro = antiMacro;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        await _antiMacro.HandleAnswerAsync(c.OnlinedCharacter, p.readString());
        await c.SendPacket(PacketCreator.enableActions());
    }
}
