namespace Application.Templates
{
    public interface ITemplate
    {
        int TemplateId { get; set; }
    }
    public abstract class AbstractTemplate: ITemplate
    {
        [GenerateIgnoreProperty]
        public int TemplateId { get; set; }
        protected AbstractTemplate(int templateId) => TemplateId = templateId;
    }
}
