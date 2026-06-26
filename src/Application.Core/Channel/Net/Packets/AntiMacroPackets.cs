using Application.Core.Channel.AntiMacro;

namespace Application.Core.Channel.Net.Packets;

/// <summary>
/// CWvsContext::OnAntiMacroResult (Opcode 0x2B)
/// 一部分在sub_7E8BA4
/// </summary>
public static class AntiMacroPackets
{
    /// <summary>
    /// 通用的错误/结果弹窗。type 0~3/7/9/11 均通过此方法发送，
    /// 客户端 OnAntiMacroResult default 分支 → ShowAntiMacroNotice → sub_7E8BA4。
    ///
    /// 各 type 对应的 SP:
    ///   0→SP_3158  1→SP_3159  2→SP_3160  3→SP_3161
    ///   7→SP_3163/3180  9→SP_3179/5565/3162  11→SP_3178
    ///
    /// <param name="type">错误/结果类型 (0~3, 7, 9, 11)</param>
    /// <param name="antiMacroType">m_nAntiMacroType: 1=道具, 2=技能(GM)</param>
    /// </summary>
    static Packet AntiMacroNotice(int type, AntiMacroType antiMacroType)
    {
        var p = OutPacket.create(SendOpcode.ANTI_MACRO_RESULT);
        p.writeByte(type);
        p.writeByte((int)antiMacroType);
        return p;
    }

    /// <summary>
    /// type 0: SP_3158 "The user cannot be found."
    /// </summary>
    public static Packet PlayerNotFound()
    {
        return AntiMacroNotice(0, 0);
    }

    /// <summary>
    /// type 1: SP_3159 "You cannot use it on a user that isn't in the middle of attack."
    /// </summary>
    public static Packet PlayerNotBattle()
    {
        return AntiMacroNotice(1, 0);
    }

    /// <summary>
    /// type 2: SP_3160 "This user has already been tested before"
    /// </summary>
    public static Packet AlreadyTested()
    {
        return AntiMacroNotice(2, 0);
    }

    /// <summary>
    /// type 3: SP_3161 "This user is currently going through the lie detector test"
    /// </summary>
    public static Packet CurrentlyTesting()
    {
        return AntiMacroNotice(3, 0);
    }

    /// <summary>
    /// type 4: "截图已保存，已通知管理人员"
    /// 客户端: SP_3168 "The screenshot has been saved. You have been notified of macro assisted program."
    /// </summary>
    public static Packet ScreenshotSaved(string filename)
    {
        var p = OutPacket.create(SendOpcode.ANTI_MACRO_RESULT);
        p.writeByte(4);
        p.writeByte(0);
        p.writeString(filename);
        return p;
    }

    /// <summary>
    /// type 5, sub=1: "[name] 使用了测谎仪测试"
    /// 客户端: SP_3167 "[name] have used the lie detector test"
    /// </summary>
    public static Packet LieDetectorUsed(string name)
    {
        var p = OutPacket.create(SendOpcode.ANTI_MACRO_RESULT);
        p.writeByte(5);
        p.writeByte(1);
        p.writeString(name);
        return p;
    }

    /// <summary>
    /// type 6: 触发展示验证码图片的测谎弹窗。
    /// 客户端 m_nAntiMacroType:
    ///   1 → 道具: dword_BF0DF8, 背景图 "Macro/Backgrnd4"
    ///   2 → 技能: dword_BF0DFC, 背景图 "MacroSkill/Backgrnd2"
    /// 两者都需要 JPEG 画布 (a1[26])，空则崩溃。
    /// JPEG 数据由服务端生成（CaptchaService），
    /// 客户端读取并渲染画面，用户输入图片中的字符提交验证。
    /// </summary>
    /// <param name="jpegData">JPEG 图片字节，不可为空。</param>
    /// <param name="antiMacroType">m_nAntiMacroType: 1=道具, 2=技能(GM)</param>
    /// <param name="setTimer">是否设置 60 秒超时定时器。</param>
    public static Packet ShowAntiMacroCaptcha(byte[] jpegData)
    {
        var p = OutPacket.create(SendOpcode.ANTI_MACRO_RESULT);
        p.writeByte(6);
        p.writeByte(2);// 1 会错位
        p.writeBool(true);
        p.writeInt(jpegData.Length);
        p.writeBytes(jpegData);
        return p;
    }

    /// <summary>
    /// type 7: 关闭弹窗 + 处罚。
    /// antiMacroType=2→SP_3180 "You will be sanctioned for using a macro-assisted program."
    /// antiMacroType≠2→SP_3163 "The Lie Detector Test confirms that you have been botting. Repeated failure of the test will result in game restrictions."
    /// </summary>
    public static Packet SanctionDialog(AntiMacroType antiMacroType)
    {
        return AntiMacroNotice(7, antiMacroType);
    }

    /// <summary>
    /// type 8, sub=2: "疑似使用外挂，截图已保存"
    /// 客户端: SP_3171 "The screenshot has been saved. It appears that you may be using a macro assisted program."
    /// </summary>
    public static Packet SuspectedMacro(string name)
    {
        var p = OutPacket.create(SendOpcode.ANTI_MACRO_RESULT);
        p.writeByte(8);
        p.writeByte(2);
        p.writeString(name);
        return p;
    }

    /// <summary>
    /// type 9: 关闭弹窗 + 通过/感谢。
    /// antiMacroType=3→SP_3179 "You have succesfully passed the Lie Detector Test. Thank you for participating!"
    /// antiMacroType=2→SP_5565 "感谢配合"
    /// antiMacroType=others→SP_3162 "Thank you for cooperating with the Lie Detector Test. You'll be rewarded 5000 mesos for not botting."
    /// </summary>
    public static Packet PassDialog(AntiMacroType antiMacroType)
    {
        return AntiMacroNotice(9, antiMacroType);
    }
    /// <summary>
    /// type 10
    /// "[name] 已通过测谎仪测试"
    /// 客户端: SP_3170 "%s_You have passed the Lie Detector Test."
    /// </summary>
    public static Packet PassedLieDetector(string name)
    {
        var p = OutPacket.create(SendOpcode.ANTI_MACRO_RESULT);
        p.writeByte(10);
        p.writeByte(2);
        p.writeString(name);
        return p;
    }

    /// <summary>
    /// type 11: SP_3178 "The user has failed the Lie Detector Test. You'll be rewarded 7,000 mesos from the user."
    /// 用于通知发起者目标未通过测谎（当前 AntiMacroResponseHandler 中失败时已通过 dropMessage 通知，
    /// 如需使用此对话框弹窗，可替换 NotifySource 中的 dropMessage）。
    ///
    /// 客户端: sub_7E8BA4 this[37]=11 → SP_3178，背景 MacroAdmin0 (SP_3153)
    /// </summary>
    public static Packet TargetFailedReward()
    {
        return AntiMacroNotice(11, AntiMacroType.Item);
    }
}
