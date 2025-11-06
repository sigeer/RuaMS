namespace Application.Scripting
{
    public class ScriptFile
    {
        public ScriptFile(string category, string name, ScriptType type)
        {
            Category = category;
            Name = name;
            Type = type;
        }

        public string Category { get; set; }
        public string Name { get; set; }

        public ScriptType Type { get; private set; }

        public string CacheKey => $"{Category}_{Name}";
        public string FileName => $"{Name}.{Type.ToString().ToLower()}";

        public void ToggleType()
        {
            Type = Type == ScriptType.Js ? ScriptType.Lua : ScriptType.Js;
        }
    }
}
