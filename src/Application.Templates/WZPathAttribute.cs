namespace Application.Templates
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class WZPathAttribute : Attribute
    {
        public WZPathAttribute(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
