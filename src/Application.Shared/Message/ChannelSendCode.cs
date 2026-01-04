namespace Application.Shared.Message
{
    public class ChannelSendCode
    {
        public const int RegisterChannel = 1;
        public const int DisconnectAll = 2;
        public const int DisconnectOne = 3;
        public const int SaveAll = 4;
        public const int SyncMap = 5;
        public const int SyncPlayer = 6;
        public const int BatchSyncPlayer = 7;
        public const int CompleteLogin = 69;

        public const int MultiChat = 8;

        public const int CreateCharacterResponse = 9;

        public const int ToggleCoupon = 10;
        // public const int OnCouponConfigUpdate = 7;

        public const int UpdateWorldConfig = 11;
        public const int SendReport = 12;
        public const int SetAutobanIgnore = 13;
        public const int SetMonitor = 14;
        public const int ReloadWorldEvents = 15;

        public const int SetTimer = 16;
        public const int RemoveTimer = 17;

        public const int BroadcastPacket = 18;

        public const int Ban = 19;
        public const int Unban = 20;
        public const int SetGmLevel = 21;

        public const int WarpPlayer = 22;
        public const int SummonPlayer = 23;

        public const int UpdateTeam = 24;
        public const int CreateTeam = 71;

        public const int UpdateGuildGp = 25;
        public const int UpdateGuildRankTitle = 26;
        public const int UpdateGuildNotice = 27;
        public const int UpdateGuildCapacity = 28;
        public const int UpdateGuildEmblem = 29;
        public const int DisbandGuild = 30;
        public const int ChangeGuildMemberRank = 31;
        public const int ExpelGuildMember = 32;
        public const int JoinGuild = 33;
        public const int LeaveGuild = 34;
        public const int SendGuildPacket = 35;
        public const int DropGuildMessage = 36;

        public const int JoinAlliance = 37;
        public const int LeaveAlliance = 38;
        public const int ExpelAllianceGuild = 39;
        public const int UpdateAllianceCapacity = 40;
        public const int UpdateAllianceNotice = 41;
        public const int UpdateAllianceRankTitle = 42;
        public const int UpdateAllianceGuildRank = 43;
        public const int UpdateAllianceLeader = 44;
        public const int DisbandAlliance = 45;

        public const int JoinChatRoom = 46;
        public const int LeaveChatRoom = 47;
        public const int SendChatRoomMessage = 48;
        public const int CreateChatRoom = 49;

        public const int DropMessage = 50;

        public const int AddBuddy = 51;
        public const int AddBuddyById = 52;
        public const int RemoveBuddy = 53;
        public const int DropBuddyMessage = 54;
        public const int GetLocation = 55;

        public const int SendWhisper = 56;

        public const int SendInvitation = 57;
        public const int AnswerInvitation = 58;

        public const int SendNewYearCard = 59;
        public const int ReceiveNewYearCard = 60;
        public const int DiscardNewYearCard = 61;

        public const int CreatePLife = 62;
        public const int RemovePLife = 63;

        public const int UseItemTV = 64;
        public const int UseItemMegaphone = 65;

        public const int CreateDueyPackage = 66;
        public const int RemoveDueyPackage = 67;
        public const int LoadDueyPackage = 68;
        public const int TakeDueyPackage = 69;
        public const int TakeDueyPackageCallback = 70;

    }
}
