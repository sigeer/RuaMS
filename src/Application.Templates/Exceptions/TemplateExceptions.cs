namespace Application.Templates.Exceptions
{
    public class TemplateNotFoundException : Exception
    {
        public TemplateNotFoundException(string provider, int templateId): base($"没有在 {provider} 下找到 Id={templateId}。")
        {
            ProviderName = provider;
            TemplateId = templateId;
        }

        public string ProviderName { get; set; }
        public int TemplateId { get; set; }
    }

    public class TemplateFormatException : Exception
    {
        public TemplateFormatException(string type, string? filePath) : base($"{type} 下找到了 {filePath} ，但是不符合读取规则。")
        {
            ProviderName = type;
            FilePath = filePath;
        }

        public string ProviderName { get; set; }
        public string? FilePath { get; set; }
    }
}
