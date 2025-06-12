namespace Application.Shared.Guild
{

    public enum AllianceUpdateResult
    {
        Success,
        PlayerNotExisted,
        GuildNotExisted,
        AllianceNotFound,
        NotGuildLeader,
        LeaderNotExisted,
        GuildAlreadyInAlliance,
        NotAllianceLeader,
        PlayerNotOnlined,
        RankLimitted,
        CapacityFull
    }
}
