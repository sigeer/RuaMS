namespace Application.Shared.Events
{
    public enum SyncCharacterTrigger
    {
        Unknown = 0,
        Logoff,
        // 包括切换频道+进出商城
        ChangeServer,
        LevelChanged,
        JobChanged,
        Auto,
        System
    }
}
