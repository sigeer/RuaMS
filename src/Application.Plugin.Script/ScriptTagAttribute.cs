namespace Application.Plugin.Script
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class ScriptTagAttribute : Attribute
    {
        public ScriptTagAttribute(string[] tags)
        {
            Tags = tags;
        }

        public string[] Tags { get; }
    }
}
