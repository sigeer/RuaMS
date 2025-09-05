namespace Application.Templates.Exceptions
{
    public class TemplateNotFoundException : Exception
    {
        public TemplateNotFoundException(string type, int templateId)
        {
            ProviderName = type;
            TemplateId = templateId;
        }

        public string ProviderName { get; set; }
        public int TemplateId { get; set; }
    }
}
