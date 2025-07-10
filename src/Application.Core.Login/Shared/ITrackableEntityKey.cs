namespace Application.Core.Login.Shared
{
    public interface ITrackableEntityKey<TKey> where TKey : notnull
    {
        TKey Id { get; }
    }

    public enum DirtyState
    {
        None = 0,
        Add = 1,
        Update = 2,
        Remove = 3
    }
}
