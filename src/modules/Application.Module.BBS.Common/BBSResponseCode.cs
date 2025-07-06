namespace Application.Module.BBS.Common
{
    public enum BBSResponseCode
    {
        Success,

        CharacterNoGuild,
        GuildNotMatched,
        ThreadNotFound,
        NoAccess,

        ExceedMaxReplyCount,
    }
}
