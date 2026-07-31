namespace Application.Templates.String
{
    public class StringQuestTemplate : StringTemplateBase
    {
        public string ParentName { get; set; }
        public StringQuestTemplate(int templateId) : base(templateId)
        {
            Name = string.Empty;
            ParentName = string.Empty;
        }
    }
}
