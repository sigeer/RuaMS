namespace Application.Shared.MapObjects
{
    public interface IGroupSoundEventMap
    {
        string SoundWin { get; set; }
        string SoundLose { get; set; }
        string GetDefaultSoundWin();
        string GetDefaultSoundLose();
    }
}
