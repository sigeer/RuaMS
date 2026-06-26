using SkiaSharp;
using System.Collections.Concurrent;

namespace Application.Core.Channel.AntiMacro;

public class CaptchaInfo
{
    public string ExpectedAnswer { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    /// <summary>发起测谎类型</summary>
    public AntiMacroType AntiMacroType { get; set; }
    /// <summary>发起者角色 ID</summary>
    public int SourceId { get; set; }
}

public class CaptchaResult
{
    public byte[] ImageBytes { get; set; } = [];
}

public class VerifyResult
{
    public bool Passed { get; set; }
    public AntiMacroType AntiMacroType { get; set; }
    public int SourceId { get; set; }
}

/// <summary>
/// 验证码服务：生成测谎弹窗所需的验证码图片（JPEG），管理待验证的答案。
///
/// 验证逻辑：用户需输入图片中显示的大写字母数字组合，不区分大小写。
/// 使用 SkiaSharp 生成图片，跨平台兼容。
/// </summary>
public class CaptchaService
{
    // 发起者-目标冷却: (sourceId, targetId) → 可再次发起的 UTC 时间
    private readonly ConcurrentDictionary<(int, int), DateTime> _cooldowns = new();
    private readonly ConcurrentDictionary<int, CaptchaInfo> _pending = new();
    private const int ImageWidth = 195;
    private const int ImageHeight = 48;
    private const int CodeLength = 5;
    private const int ExpirySeconds = 60;
    private static readonly TimeSpan CooldownDuration = TimeSpan.FromHours(1);

    /// <summary>
    /// 检查发起者-目标对是否在冷却中。
    /// </summary>
    public bool IsOnCooldown(int sourceId, int targetId)
    {
        return _cooldowns.TryGetValue((sourceId, targetId), out var until) && DateTime.UtcNow < until;
    }

    /// <summary>
    /// 记录发起者-目标冷却。
    /// </summary>
    public void SetCooldown(int sourceId, int targetId)
    {
        _cooldowns[(sourceId, targetId)] = DateTime.UtcNow + CooldownDuration;
    }

    /// <summary>
    /// 为目标角色创建验证码。若该角色已有待验证的测谎，则返回 null。
    /// </summary>
    /// <param name="targetCharacterId">目标角色 ID</param>
    /// <param name="antiMacroType">m_nAntiMacroType: 1=道具, 2=技能(GM)</param>
    /// <returns>验证码结果，已有待验证测谎则返回 null</returns>
    public CaptchaResult? CreateCaptcha(int targetCharacterId, AntiMacroType antiMacroType = AntiMacroType.Item, int sourceId = 0)
    {
        var code = GenerateRandomCode();

        var info = new CaptchaInfo
        {
            ExpectedAnswer = code,
            ExpiresAt = DateTime.UtcNow.AddSeconds(ExpirySeconds),
            AntiMacroType = antiMacroType,
            SourceId = sourceId
        };

        if (!_pending.TryAdd(targetCharacterId, info))
            return null; // 该角色已有待验证的测谎

        var jpegBytes = GenerateCaptchaImage(code);

        return new CaptchaResult
        {
            ImageBytes = jpegBytes
        };
    }

    /// <summary>
    /// 检查目标角色是否已有未过期的待验证测谎。
    /// </summary>
    public bool HasPending(int targetCharacterId)
    {
        return _pending.ContainsKey(targetCharacterId);
    }

    /// <summary>
    /// 消费待验证测谎并返回结果。无待验证测谎返回 null。
    /// </summary>
    public VerifyResult? VerifyAnswer(int targetCharacterId, string? userAnswer)
    {
        if (!_pending.TryRemove(targetCharacterId, out var info))
            return null;

        return new VerifyResult
        {
            Passed = DateTime.UtcNow <= info.ExpiresAt
                  && string.Equals(userAnswer?.Trim(), info.ExpectedAnswer, StringComparison.OrdinalIgnoreCase),
            AntiMacroType = info.AntiMacroType,
            SourceId = info.SourceId,
        };
    }

    public bool TryRemove(int targetCharacterId)
    {
        return _pending.TryRemove(targetCharacterId, out _);
    }

    private static string GenerateRandomCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var code = new char[CodeLength];
        for (var i = 0; i < CodeLength; i++)
            code[i] = chars[Random.Shared.Next(chars.Length)];
        return new string(code);
    }

    private static byte[] GenerateCaptchaImage(string code)
    {
        using var surface = SKSurface.Create(new SKImageInfo(ImageWidth, ImageHeight));
        var canvas = surface.Canvas;

        // 背景
        canvas.Clear(new SKColor(240, 240, 245));

        // 干扰线条
        for (var i = 0; i < 6; i++)
        {
            using var paint = new SKPaint
            {
                Color = new SKColor(
                    (byte)Random.Shared.Next(180, 220),
                    (byte)Random.Shared.Next(180, 220),
                    (byte)Random.Shared.Next(180, 220)),
                StrokeWidth = 1,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };
            canvas.DrawLine(
                Random.Shared.Next(0, ImageWidth), Random.Shared.Next(0, ImageHeight),
                Random.Shared.Next(0, ImageWidth), Random.Shared.Next(0, ImageHeight),
                paint);
        }

        // 干扰噪点
        for (var i = 0; i < 80; i++)
        {
            using var paint = new SKPaint
            {
                Color = new SKColor(
                    (byte)Random.Shared.Next(100, 200),
                    (byte)Random.Shared.Next(100, 200),
                    (byte)Random.Shared.Next(100, 200)),
                StrokeWidth = 1,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawPoint(Random.Shared.Next(0, ImageWidth), Random.Shared.Next(0, ImageHeight), paint);
        }

        // 逐个字符绘制，旋转随机角度
        using var typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic);
        using var fontPaint = new SKPaint
        {
            Typeface = typeface,
            TextSize = 22,
            IsAntialias = true
        };

        canvas.Save();
        for (var i = 0; i < code.Length; i++)
        {
            var angle = Random.Shared.Next(-20, 21);
            var x = (ImageWidth / (code.Length + 1)) * (i + 0.3f);
            var y = Random.Shared.Next(35, 45);

            fontPaint.Color = new SKColor(
                (byte)Random.Shared.Next(30, 100),
                (byte)Random.Shared.Next(30, 100),
                (byte)Random.Shared.Next(60, 140));

            canvas.RotateDegrees(angle, x, y);
            canvas.DrawText(code[i].ToString(), x, y, fontPaint);
            canvas.RotateDegrees(-angle, x, y);
        }
        canvas.Restore();

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);
        return data.ToArray();
    }
}
