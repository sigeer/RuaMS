namespace Application.Shared.Login
{
    [Flags]
    public enum BanLevel
    {
        OnlyAccount = 0,
        IP = 1 << 0,
        Mac = 1 << 1,
        Hwid = 1 << 2,

        All = OnlyAccount | IP | Mac | Hwid
    }
}
