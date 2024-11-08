namespace Application.Core.Gameplay
{
    [Flags]
    public enum PickupCheckFlags
    {
        None = 0,
        CoolDown,
        Owner
    }
}
