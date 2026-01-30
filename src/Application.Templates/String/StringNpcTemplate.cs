namespace Application.Templates.String
{
    public class StringNpcTemplate : AbstractTemplate
    {
        public string Name { get; set; }
        public string? Func { get; set; }
        public string DefaultTalk0 { get; set; }
        public string DefaultTalk1 { get => _defaultTalk1 ??= DefaultTalk0; set => _defaultTalk1 = value; }

        string? _defaultTalk1;

        public StringNpcTemplate(int templateId) : base(templateId)
        {
            Name = Defaults.WZ_MissingNo;
            DefaultTalk0 = "(...)";
        }
    }
}
