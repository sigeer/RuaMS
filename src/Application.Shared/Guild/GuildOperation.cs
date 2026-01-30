namespace Application.Shared.Team
{
    public enum GuildUpdateResult
    {
        Success,
        PlayerNotExisted,
        PlayerNotOnlined,
        NoGuild,
        GuildNotExisted ,
        GuildFull,

        MasterRankFail,

        Join_AlreadyInGuild,
        LeaderCannotLeave,

        Create_NameDumplicate,
        Create_AlreadyInGuild,
        Create_LeaderRequired,
        Create_MapRequired,
        Create_MemberNotHere,
        Create_MemberAlreadyInGuild,
        Create_MesoNotEnough,
    }
}
