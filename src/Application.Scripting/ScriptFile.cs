namespace Application.Scripting
{
    public class ScriptFile
    {
        public ScriptFile(string category, string name) : this(category, name, ScriptType.Js)
        {
        }

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

        public void UpdateType(ScriptType type)
        {
            Type = type;
        }
    }
}
