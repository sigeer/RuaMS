namespace Application.Utility.Tasks
{
    public class TimerUtils
    {
        public const string DefaultGroup = "DEFAULT";
        public static JobKey GenerateKey(string group, string name) => new JobKey(group, name);
        public static JobKey GenerateKey(string name) => GenerateKey(DefaultGroup, name);
    }

    public record JobKey(string Group, string Name)
    {
        public override string ToString()
        {
            return $"{Group}.{Name}";
        }
    };
}
