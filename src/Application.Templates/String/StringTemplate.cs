namespace Application.Templates.String
{
    /// <summary>
    /// Item, Skill, Mob
    /// </summary>
    public sealed class StringTemplate : StringTemplateBase
    {

        [WZPath("desc")]
        public string? Description { get; set; }

        [WZPath("msg")]
        public string? Message { get; set; }

        public StringTemplate(int templateId)
            : base(templateId)
        {
        }
    }
}
