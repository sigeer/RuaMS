namespace Application.Utility.Tickables
{
    public interface ITickable
    {
        long Next { get; }
        long Period { get; }
        bool Disabled { get; set; }
        void OnTick(long now);
    }
}
