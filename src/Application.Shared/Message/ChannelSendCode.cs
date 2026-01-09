namespace Application.Shared.Message
{
    public enum ChannelSendCode
    {
        RegisterChannel,

        DisconnectAll,
        DisconnectOne,
        SaveAll,
        SyncMap,
        SyncPlayer,
        BatchSyncPlayer,
        CompleteLogin,

        MultiChat,
        CreateCharacterResponse,
        ToggleCoupon,
        OnCouponConfigUpdate,
        UpdateWorldConfig,
        SendReport,
        SetAutobanIgnore,
        SetMonitor,
        ReloadWorldEvents,
        SetTimer,
        RemoveTimer,
        BroadcastPacket,
        Ban,
        Unban,

        Jail,
        Unjail,

        SetGmLevel,
        WarpPlayer,
        SummonPlayer,

        UpdateTeam,
        CreateTeam,

        CreateGuild,
        UpdateGuildGp,
        UpdateGuildRankTitle,
        UpdateGuildNotice,
        UpdateGuildCapacity,
        UpdateGuildEmblem,
        DisbandGuild,
        ChangeGuildMemberRank,
        ExpelGuildMember,
        JoinGuild,
        LeaveGuild,
        SendGuildPacket,
        DropGuildMessage,

        CreateAlliance,
        JoinAlliance,
        LeaveAlliance,
        ExpelAllianceGuild,
        UpdateAllianceCapacity,
        UpdateAllianceNotice,
        UpdateAllianceRankTitle,
        UpdateAllianceGuildRank,
        UpdateAllianceLeader,
        DisbandAlliance,
        AllianceBroadcastPlayerInfo,

        JoinChatRoom,
        LeaveChatRoom,
        SendChatRoomMessage,
        CreateChatRoom,

        DropMessage,

        AddBuddy,
        AddBuddyById,
        RemoveBuddy,
        DropBuddyMessage,
        GetLocation,
        SendWhisper,

        SendInvitation,
        AnswerInvitation,

        SendNewYearCard,
        ReceiveNewYearCard,
        DiscardNewYearCard,

        CreatePLife,
        RemovePLife,

        UseItemTV,
        UseItemMegaphone,

        CreateDueyPackage,
        RemoveDueyPackage,
        LoadDueyPackage,
        TakeDueyPackage,
        TakeDueyPackageCallback,

    }
}
