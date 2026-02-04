namespace Application.Templates.Etc
{
    public class ScriptInfoTemplate : AbstractTemplate
    {
        public ScriptInfoTemplate() : base(0)
        {
            Name = string.Empty;
            Value = string.Empty;
        }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
