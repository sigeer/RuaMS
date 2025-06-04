namespace Application.Shared.Team
{
    public enum UpdateTeamCheckResult
    {
        Success ,

        Join_TeamMemberFull,
        Join_HasTeam,
        TeamNotExsited,

        Join_InnerError,

        Expel_NotLeader,
        Expel_InnerError,

        Leave_InnerError,
        Disband_NotLeader,

        ChangeLeader_NotLeader,
        ChangeLeader_MemberNotExsited
    }
}
