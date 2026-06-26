using Application.Core.Channel.AntiMacro;
using Application.Core.Channel.Net.Packets;
using Application.Core.Net;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/// <summary>
/// 处理 CUserLocal::DoAntiMacroSkill (RecvOpcode 104)。
/// 当 GM 使用 ANTI_MACRO 技能 (9001009) 时触发。
/// </summary>
public class AntiMacroSkillUseRequestHandler : ChannelHandlerBase
{
    private readonly AntiMacroService _antiMacro;

    public AntiMacroSkillUseRequestHandler(AntiMacroService antiMacro)
    {
        _antiMacro = antiMacro;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;

        if (!chr.isAlive() || !chr.isGM())
        {
            await c.SendPacket(PacketCreator.enableActions());
            return;
        }

        var targetName = p.readString();

        var target = chr.getMap().getCharacterByName(targetName);
        if (target == null || !target.IsOnlined || target == chr)
        {
            await c.SendPacket(AntiMacroPackets.PlayerNotFound());
            return;
        }

        await _antiMacro.SendAntiMacroAsync(chr, target, AntiMacroType.AdminSkill);
    }
}
