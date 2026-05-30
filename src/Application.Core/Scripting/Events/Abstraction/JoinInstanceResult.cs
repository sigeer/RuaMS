namespace Application.Core.scripting.Events.Abstraction
{
    public enum JoinInstanceResult : byte
    {
        Success,

        StopRecruitment,
        Banned,
        RoomFull,
        ChannelFull,
        LevelOutOfRange,

        Unknown
    }
}
