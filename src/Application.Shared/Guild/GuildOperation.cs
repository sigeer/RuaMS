namespace Application.Shared.Team
{
    public enum GuildOperation
    {
        AddMember,
        ChangeRank,
        ExpelMember,
        Leave,
        MemberLevelChanged,
        MemberJobChanged,
        MemberLogin,
        MemberLogoff
    }

    public enum GuildInfoOperation
    {
        ChangeName,
        ChangeEmblem,
        ChangeRankTitle,
        ChangeNotice,
        IncreaseCapacity,
        Disband,
    }
}
