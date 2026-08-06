namespace Application.Templates.String
{
    public class StringNpcTemplate : StringTemplateBase
    {
        public string? Func { get; set; }
        /// <summary>
        /// d0
        /// </summary>
        public string DefaultTalk0 { get; set; }
        /// <summary>
        /// d1
        /// </summary>
        public string DefaultTalk1 { get => _defaultTalk1 ??= DefaultTalk0; set => _defaultTalk1 = value; }

        string? _defaultTalk1;

        public StringNpcTemplate(int templateId) : base(templateId)
        {
            DefaultTalk0 = "(...)";
        }
    }
}
