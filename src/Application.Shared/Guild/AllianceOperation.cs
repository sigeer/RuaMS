namespace Application.Shared.Guild
{
    public enum AllianceOperation
    {
        LeaveAlliance,
        ExpelGuild,
        Join,
        MemberLogin,
        MemberUpdate,

        IncreasePlayerRank,
        DecreasePlayerRank,
        ChangeAllianceLeader,
        IncreaseCapacity,
        Disband,
        ChangeRankTitle,
        ChangeNotice,
    }

    public enum AllianceUpdateResult
    {
        Success,
        PlayerNotExisted,
        AllianceNotFound,
        GuildNotExsited,
        NotGuildLeader,
        LeaderNotExisted,
        GuildAlreadyInAlliance,
        NotAllianceLeader,
        PlayerNotOnlined,
    }
}
