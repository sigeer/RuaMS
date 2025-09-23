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

    public class TemplateFormatException : Exception
    {
        public TemplateFormatException(string type, string filePath)
        {
            ProviderName = type;
            FilePath = filePath;
        }

        public string ProviderName { get; set; }
        public string FilePath { get; set; }
    }
}
