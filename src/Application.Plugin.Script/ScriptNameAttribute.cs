namespace Application.Plugin.Script
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class ScriptNameAttribute : Attribute
    {
        public ScriptNameAttribute(params string[] name)
        {
            Name = name;
        }

        public string[] Name { get; set; }
    }
}
