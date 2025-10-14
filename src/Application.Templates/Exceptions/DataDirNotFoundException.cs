namespace Application.Templates.Exceptions
{
    public class DataDirNotFoundException : DirectoryNotFoundException
    {
        public DataDirNotFoundException()
        {
        }

        public DataDirNotFoundException(string? message) : base(message)
        {
        }
    }
}
