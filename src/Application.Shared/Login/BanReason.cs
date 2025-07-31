using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Login
{
    //<HACK/BOT/AD/HARASS/CURSE/SCAM/MISCONDUCT/SELL/ICASH/TEMP/GM/IPROGRAM/MEGAPHONE>
    public enum BanReason
    {
        /// <summary>
        /// 使用外挂、修改器、作弊程序
        /// </summary>
        HACK,
        /// <summary>
        /// 脚本
        /// </summary>
        BOT,
        /// <summary>
        /// 广告
        /// </summary>
        AD,
        /// <summary>
        /// 骚扰
        /// </summary>
        HARASS,
        /// <summary>
        /// 骂人、辱骂、带有侮辱性言语
        /// </summary>
        CURSE,
        /// <summary>
        /// 诈骗
        /// </summary>
        SCAM,
        /// <summary>
        /// 不当
        /// </summary>
        MISCONDUCT,
        /// <summary>
        /// 售卖账号/道具/金币
        /// </summary>
        SELL,
        /// <summary>
        /// 非法获取充值点
        /// </summary>
        ICASH,
        /// <summary>
        /// 临时封禁
        /// </summary>
        TEMP,
        /// <summary>
        /// GM手动封禁
        /// </summary>
        GM,
        /// <summary>
        /// 使用了未授权或非法的程序插件
        /// </summary>
        IPROGRAM,
        /// <summary>
        /// 滥用大喇叭 刷屏、广告、辱骂
        /// </summary>
        MEGAPHONE
    }
}
