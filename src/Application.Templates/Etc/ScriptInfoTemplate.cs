namespace Application.Templates.Etc
{
    public class ScriptInfoTemplate : AbstractTemplate
    {
        public ScriptInfoTemplate(int idx) : base(idx)
        {
            Name = string.Empty;
            Value = string.Empty;
        }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
