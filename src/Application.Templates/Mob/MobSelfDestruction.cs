namespace Application.Templates.Mob
{
    public record MobSelfDestruction(int ActionType, int RemoveAfter, int Hp);

    public class MobSelfDestructionBuilder
    {
        public int ActionType { get; set; }
        public int RemoveAfter { get; set; } = -1;
        public int Hp { get; set; } = -1;

        public MobSelfDestruction Build() => new MobSelfDestruction(ActionType, RemoveAfter, Hp);
    }
}
