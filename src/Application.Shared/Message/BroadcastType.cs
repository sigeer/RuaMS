namespace Application.Shared.Message
{
    public class BroadcastType
    {
        public const string Whisper_Chat = "Whisper_Chat";

        public const string Buddy_Chat = "Buddy_Chat";
        public const string Buddy_Added = "Buddy_Added";
        public const string Buddy_Delete = "Buddy_Delete";
        public const string Buddy_NotifyChannel = "Buddy_NotifyChannel";
        public const string Buddy_NoticeMessage = "Buddy_NoticeMessage";

        public const string SendPlayerDisconnectAll = "SendPlayerDisconnectAll";
        public const string SendPlayerDisconnect = "SendPlayerDisconnect";
        public const string SendWrapPlayerByName = "SendWrapPlayerByName";

        public const string OnGmLevelSet = "OnGmLevelSet";
        public const string BroadcastBan = "BroadcastBan";
        public const string OnAutoBanIgnoreChangedNotify = "OnAutoBanIgnoreChangedNotify";
        public const string OnMonitorChangedNotify = "OnMonitorChangedNotify";

        public const string OnReportReceived = "OnReportReceived";

        public const string PS_OnHasHiredMerchant = "PS_OnHasHiredMerchant";
        public const string OnCashItemPurchased = "OnCashItemPurchased";

        public const string OnPLifeCreated = "OnPLifeCreated";
        public const string OnPLifeRemoved = "OnPLifeRemoved";

        public const string OnItemMegaphone = "OnItemMegaphone";
        public const string OnTVMessage = "OnTVMessage";
        public const string OnTVMessageFinish = "OnTVMessageFinish";
        public const string OnMessage = "OnMessage";
        public const string OnSetFly = "OnSetFly";
        public const string OnEventsReloaded = "OnEventsReloaded";
        public const string OnShutdown = "OnShutdown";
        public const string OnWorldConfigUpdate = "OnWorldConfigUpdate";
        public const string OnNoteSend = "OnNoteSend";

        public const string OnInvitationSend = "OnInvitationSend";
        public const string OnInvitationAnswer = "OnInvitationAnswer";

        public const string OnCouponConfigUpdate = "OnCouponConfigUpdate";

        public const string OnTeamUpdate = "OnTeamUpdate";

        public const string OnDropMessage = "OnDropMessage";
        public const string OnMultiChat = "OnMultiChat";

        public const string OnChatRoomMessageSend = "OnChatRoomMessageSend";
        public const string OnJoinChatRoom = "OnJoinChatRoom";
        public const string OnLeaveChatRoom = "OnLeaveChatRoom";

        public const string OnGuildGpUpdate = "OnGuildGpUpdate";
        public const string OnGuildNoticeUpdate = "OnGuildNoticeUpdate";
        public const string OnPlayerLeaveGuild = "OnPlayerLeaveGuild";
        public const string OnPlayerJoinGuild = "OnPlayerJoinGuild";
        public const string OnGuildExpelMember = "OnGuildExpelMember";
        public const string OnGuildRankChanged = "OnGuildRankChanged";
        public const string OnGuildRankTitleUpdate = "OnGuildRankTitleUpdate";
        public const string OnGuildEmblemUpdate = "OnGuildEmblemUpdate";
        public const string OnGuildCapacityUpdate = "OnGuildCapacityUpdate";
        public const string OnGuildDisband = "OnGuildDisband";

        public const string OnGuildJoinAlliance = "OnGuildJoinAlliance";
        public const string OnGuildLeaveAlliance = "OnGuildLeaveAlliance";
        public const string OnAllianceDisband = "OnAllianceDisband";
        public const string OnAllianceRankChange = "OnAllianceRankChange";
        public const string OnAllianceRankTitleUpdate = "OnAllianceRankTitleUpdate";
        public const string OnAllianceCapacityUpdate = "OnAllianceCapacityUpdate";
        public const string OnAllianceExpelGuild = "OnAllianceExpelGuild";
        public const string OnAllianceNoticeUpdate = "OnAllianceNoticeUpdate";
        public const string OnAllianceChangeLeader = "OnAllianceChangeLeader";


        public const string OnPlayerLevelChanged = "OnPlayerLevelChanged";
        public const string OnPlayerJobChanged = "OnPlayerJobChanged";
        public const string OnPlayerLoginOff = "OnPlayerLoginOff";

        public const string OnNewYearCardSend = "OnNewYearCardSend";
        public const string OnNewYearCardReceived = "OnNewYearCardReceived";
        public const string OnNewYearCardNotify = "OnNewYearCardNotify";
        public const string OnNewYearCardDiscard = "OnNewYearCardDiscard";
    }
}
