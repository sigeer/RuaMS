namespace Application.Shared.Invitations
{
    public enum InviteTypeEnum
    {
        FAMILY,
        FAMILY_SUMMON,
        /// <summary>
        /// 聊天室、可跨频道
        /// </summary>
        MESSENGER,
        /// <summary>
        /// 交易、仅同一地图
        /// </summary>
        TRADE,
        /// <summary>
        /// 组队、可跨频道
        /// </summary>
        PARTY,
        /// <summary>
        /// 家族、可跨频道
        /// </summary>
        GUILD,
        /// <summary>
        /// 联盟、可跨频道
        /// </summary>
        ALLIANCE,
    }

    public enum InviteResultType
    {
        ACCEPTED,
        DENIED,
        NOT_FOUND
    }

    public class InviteTypes
    {
        public const string Trade = "Trade";
        public const string Party = "Party";
        public const string Guild = "Guild";
        public const string Alliance = "Alliance";
        public const string Messenger = "Messenger";
    }
}
