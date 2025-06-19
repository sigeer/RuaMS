namespace Application.Shared.Invitations
{
    public enum GuildResponse
    {
        Success = 0,

        NOT_IN_CHANNEL = 0x2a,
        ALREADY_IN_GUILD = 0x28,
        NOT_IN_GUILD = 0x2d,
        NOT_FOUND_INVITE = 0x2e,

        MANAGING_INVITE = 0x36,
        DENIED_INVITE = 0x37

    }
}
