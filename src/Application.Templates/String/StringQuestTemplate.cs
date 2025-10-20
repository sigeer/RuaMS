namespace Application.Templates.String
{
    public class StringQuestTemplate : AbstractTemplate
    {
        public string Name { get; set; }
        public string ParentName { get; set; }
        public StringQuestTemplate(int templateId) : base(templateId)
        {
            Name = string.Empty;
            ParentName = string.Empty;
        }
    }
}
