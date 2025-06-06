namespace Application.Shared.Login
{
    public enum LoginResultCode
    {
        Success = 0,
        /// <summary>
        /// 账号不存在
        /// </summary>
        Fail_AccountNotExsited = 5,
        /// <summary>
        /// 密码错误
        /// </summary>
        Fail_IncorrectPassword = 4,
        /// <summary>
        /// 已经登录了
        /// </summary>
        Fail_AlreadyLoggedIn = 7,
        /// <summary>
        /// 失败次数过多
        /// </summary>
        Fail_Count = 6,
        /// <summary>
        /// id小于0的account，可能有什么特殊的
        /// </summary>
        Fail_SpecialAccount = 15,
        Fail_Banned = 3,

        Fail_Error8 = 8,
        Fail_Error9 = 9,
        Fail_Error10 = 10,
        Fail_Error16 = 16,
        Fail_Error13 = 13,
        Fail_Error17 = 17,

        /// <summary>
        /// 未同意协议
        /// </summary>
        Fail_Agreement = 23,

        MigrateBCrypto = -10,
        CheckTOS = -23
    }

    public class LoginStage
    {
        /// <summary>
        /// 账号尚未登录
        /// </summary>
        public const int LOGIN_NOTLOGGEDIN = 0;
        /// <summary>
        /// 账号正在切换服务器：由登录服务器 -> 频道服务器
        /// </summary>
        public const int LOGIN_SERVER_TRANSITION = 1;
        /// <summary>
        /// 账号已登录
        /// </summary>
        public const int LOGIN_LOGGEDIN = 2;
        /// <summary>
        /// 玩家正在切换服务器：由频道服务器->频道服务器
        /// </summary>

        public const int PlayerServerTransition = 3;
        /// <summary>
        /// 小于0 ：相应的playerid表示正在登录的玩家id
        /// </summary>
        public const int PlayerLoggedIn = -999999;
    }
}
