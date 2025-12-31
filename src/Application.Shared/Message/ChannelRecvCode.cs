namespace Application.Shared.Message
{
    public class ChannelRecvCode
    {
        public const int RegisterChannel = 1;
        public const int UnregisterChannel = 2;
        public const int DisconnectAll = 3;
        public const int InvokeDisconnectPlayer = 4;

        public const int SaveAll = 5;
        public const int InvokeSetGmLevel = 6;
        public const int BanPlayer = 7;
        public const int Unban = 8;
        public const int InvokeAutoBanIgnore = 9;
        public const int InvokeMonitor = 10;

        public const int InvokeNoteMessage = 11;

        public const int SummonPlayer = 12;
        public const int WarpPlayer = 13;

        public const int OnPlayerJobChanged = 14;
        public const int OnPlayerLevelChanged = 15;
        public const int OnPlayerServerChanged = 16;

        public const int DropTextMessage = 17;
        public const int MultiChat = 18;

        public const int HandleFullPacket = 19;

        public const int OnTeamUpdate = 20;

        public const int OnGuildGpUpdate = 21;
        public const int OnGuildRankTitleUpdate = 22;
        public const int OnGuildNoticeUpdate = 23;
        public const int OnGuildCapacityUpdate = 24;
        public const int OnGuildEmblemUpdate = 25;
        public const int OnGuildDisband = 26;
        public const int OnGuildRankChanged = 27;
        public const int OnGuildExpelMember = 28;
        public const int OnPlayerJoinGuild = 29;
        public const int OnPlayerLeaveGuild = 30;

        public const int OnGuildJoinAlliance = 31;
        public const int OnGuildLeaveAlliance = 32;
        public const int OnAllianceExpelGuild = 33;
        public const int OnAllianceCapacityUpdate = 34;
        public const int OnAllianceNoticeUpdate = 35;
        public const int OnAllianceRankTitleUpdate = 36;
        public const int OnAllianceMemberRankChanged = 37;
        public const int OnAllianceLeaderChanged = 38;
        public const int OnAllianceDisband = 39;

        public const int OnJoinChatRoom = 40;
        public const int OnLeaveChatRoom = 41;
        public const int OnChatRoomMessageReceived = 42;

        public const int CreateCharacter = 43;
        public const int OnCouponConfigUpdate = 44;
        public const int OnWorldConfigUpdate = 45;

        public const int OnBuddyAdd = 46;
        public const int OnBuddyRemove = 47;
        public const int OnBuddyNotify = 48;
        public const int OnBuddyLocation = 49;

        public const int OnWhisper = 50;

        public const int OnInvitationSent = 51;
        public const int OnInvitationAnswered = 52;

        public const int OnNewYearCardSent = 53;
        public const int OnNewYearCardReceived = 54;
        public const int OnNewYearCardNotify = 55;
        public const int OnNewYearCardDiscard = 56;

        public const int OnPlifeCreated = 57;
        public const int OnPlifeRemoved = 58;

        public const int HandleReportReceived = 59;

        public const int HandleSetTimer = 60;
        public const int HandleRemoveTimer = 61;

        public const int HandleWorldEventReload = 62;

        public const int HandleItemMegaphone = 63;
        public const int HandleTVMessageStart = 64;
        public const int HandleTVMessageFinish = 65;

        public const int TakeDueyPackage = 66;
        public const int TakeDueyPackageCallback = 67;
        public const int CreateDueyPackage = 68;
        public const int DeleteDueyPackage = 69;
        public const int LoginNotifyDueyPackage = 70;
        public const int LoadDueyPackage = 71;
    }
}
