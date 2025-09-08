namespace Application.Templates
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class WZPathAttribute : Attribute
    {
        public WZPathAttribute(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
