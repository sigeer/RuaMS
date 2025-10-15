namespace Application.Templates.Exceptions
{
    public class ProviderNotFoundException : Exception
    {
        public string ProviderName { get; }
        public ProviderNotFoundException(string type)
        {
            ProviderName = type;
        }

        public ProviderNotFoundException(string type, string message) : base(message)
        {
            ProviderName = type;
        }
    }

    public class ProviderDuplicateException : Exception
    {
        public string ProviderName { get; }
        public ProviderDuplicateException(string type)
        {
            ProviderName = type;
        }
    }

    public class ImgNotFound : FileNotFoundException
    {
        public ImgNotFound()
        {
        }

        public ImgNotFound(string? message) : base(message)
        {
        }

        public ImgNotFound(string? message, string? fileName) : base(message, fileName)
        {
        }
    }
}
