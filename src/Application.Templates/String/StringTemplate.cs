namespace Application.Templates.String
{
    public sealed class StringTemplate : AbstractTemplate
    {
        [WZPath("name")]
        public string Name { get; set; }

        [WZPath("desc")]
        public string? Description { get; set; }

        [WZPath("msg")]
        public string? Message { get; set; }

        public StringTemplate(int templateId)
            : base(templateId)
        {
            Name = "";
        }
    }
}
