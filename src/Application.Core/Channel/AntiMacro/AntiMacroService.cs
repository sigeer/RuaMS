using Application.Core.Channel.Net.Packets;
using server.movement;
using SystemProto;
using tools;

namespace Application.Core.Channel.AntiMacro;

/// <summary>
/// 测谎业务逻辑：协调 CaptchaService、处罚、通知发起者等。
/// </summary>
public class AntiMacroService
{
    private readonly WorldChannelServer _server;
    private readonly CaptchaService _captcha;

    public AntiMacroService(WorldChannelServer server, CaptchaService captcha)
    {
        _server = server;
        _captcha = captcha;
    }

    /// <summary>
    /// 发包 + 启动超时检测。
    /// </summary>
    public async Task SendAntiMacroAsync(Player sender, Player target, AntiMacroType type, Func<Task>? onSuccess = null)
    {
        if (!StanceUtils.IsBattle((byte)target.getStance()))
        {
            await sender.SendPacket(AntiMacroPackets.PlayerNotBattle());
            return;
        }

        if (type == AntiMacroType.Item)
        {
            // ---- 校验冷却 ----
            if (_captcha.IsOnCooldown(sender.Id, target.Id))
            {
                await sender.SendPacket(AntiMacroPackets.AlreadyTested());
                return;
            }
        }

        if (_captcha.HasPending(target.Id))
        {
            await sender.SendPacket(AntiMacroPackets.CurrentlyTesting());
            return;
        }

        var captcha = _captcha.CreateCaptcha(target.Id, type, sourceId: sender.Id);
        if (captcha == null)
        {
            await sender.SendPacket(PacketCreator.enableActions());
            return;
        }

        await target.SendPacket(AntiMacroPackets.ShowAntiMacroCaptcha(captcha.ImageBytes));

        if (onSuccess != null)
        {
            await onSuccess();
        }
        if (type == AntiMacroType.Item)
        {
            _captcha.SetCooldown(sender.Id, target.Id);
            await target.SendPacket(AntiMacroPackets.LieDetectorUsed(sender.Name));
        }

        var tId = target.Id;
        var tName = target.Name;
        var senderId = sender.Id;
        _ = Task.Run(async () =>
        {
            await Task.Delay(60_000);

            if (_captcha.TryRemove(tId))
            {
                await _server.Transport.AntiMacroNotify(new SystemProto.AntiMacroNotifyMessage
                {
                    ReporterId = senderId,
                    VictimId = tId,
                    Reason = "测谎超时",
                    Passed = false,
                    Type = (int)type
                });
            }
        });
    }

    /// <summary>
    /// 处理答案提交。
    /// </summary>
    public async Task HandleAnswerAsync(Player chr, string answer)
    {
        var result = _captcha.VerifyAnswer(chr.Id, answer);
        if (result == null)
        {
            return;
        }

        await _server.Transport.AntiMacroNotify(new SystemProto.AntiMacroNotifyMessage
        {
            ReporterId = result.SourceId,
            VictimId = chr.Id,
            Reason = "未通过测谎",
            Passed = result.Passed,
            Type = (int)result.AntiMacroType
        });
    }

    /// <summary>
    /// 测谎结束回调
    /// </summary>
    public async Task PenalizeAsync(AntiMacroNotifyMessage res)
    {
        var type = (AntiMacroType)res.Type;

        if (!res.Passed)
        {
            await _server.SendToPlayerAsync(res.ReporterId, async chr =>
            {
                await chr.SendPacket(AntiMacroPackets.TargetFailedReward());
                await chr.GainMeso(7000, GainItemShow.ShowInChat);
            });
        }

        await _server.SendToPlayerAsync(res.VictimId, async chr =>
        {
            if (res.Passed)
            {
                await chr.SendPacket(AntiMacroPackets.PassedLieDetector(chr.Name));
                await chr.SendPacket(AntiMacroPackets.PassDialog(type));
                if (type == AntiMacroType.Item)
                    await chr.GainMeso(5000);
            }
            else
            {
                await chr.SendPacket(AntiMacroPackets.SuspectedMacro(chr.Name));
                await chr.SendPacket(AntiMacroPackets.SanctionDialog(type));
            }
        });
    }
}
