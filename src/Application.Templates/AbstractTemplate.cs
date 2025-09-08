namespace Application.Templates
{
    public abstract class AbstractTemplate
    {
        [GenerateIgnoreProperty]
        public int TemplateId { get; set; }
        protected AbstractTemplate(int templateId) => TemplateId = templateId;
    }
}
