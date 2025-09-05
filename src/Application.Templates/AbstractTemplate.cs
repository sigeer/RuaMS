namespace Application.Templates
{
    public abstract class AbstractTemplate
    {
        public int TemplateId { get; set; }
        protected AbstractTemplate(int templateId) => TemplateId = templateId;
    }
}
