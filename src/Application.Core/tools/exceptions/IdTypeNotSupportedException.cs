namespace tools.exceptions;

public class IdTypeNotSupportedException : Exception
{
    public IdTypeNotSupportedException() : base("The given ID type is not supported")
    {

    }

    public IdTypeNotSupportedException(string message) : base(message)
    {
    }
}
